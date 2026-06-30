using HRMS.Constants;
using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace HRMS.Controllers
{
  
    public class PayrollController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public PayrollController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LIST
        [Authorize]
        public IActionResult Index()
        {
            if (User.IsInRole("Admin") || User.IsInRole("HR"))
            {
                var payrolls = _context.Payrolls
                    .Include(x => x.Employee)
                    .OrderByDescending(x => x.PayMonth)
                    .ToList();

                return View(payrolls);
            }

            var employeeIdClaim =
                User.FindFirstValue(CustomClaimTypes.EmployeeId);

            if (!int.TryParse(employeeIdClaim, out int employeeId))
            {
                return Challenge();
            }

            var myPayrolls = _context.Payrolls
                .Include(x => x.Employee)
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.PayMonth)
                .ToList();

            return View(myPayrolls);

        }

        // CREATE GET
        [Authorize(Roles = "Admin,HR")]
        public IActionResult Create()
        {
            ViewBag.Employees =
                new SelectList(_context.Employees,
                "EmployeeId",
                "FirstName");

            return View();
        }

        // CREATE POST
        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Payroll payroll)
        {
            payroll.GrossSalary =
payroll.BasicSalary +
payroll.HRA +
payroll.DA;

            payroll.PF = payroll.BasicSalary * 12 / 100;

            payroll.ProfessionalTax = 200;

            payroll.NetSalary =
                payroll.GrossSalary -
                payroll.PF -
                payroll.ProfessionalTax;

            payroll.GeneratedDate = DateTime.Now;

            _context.Payrolls.Add(payroll);
            _context.SaveChanges();

            AuditLog log = new AuditLog()
            {
                //UserName = HttpContext.Session.GetString("UserName"),
                UserName = User.Identity?.Name,
                ActionType = "Generate",
                ModuleName = "Payroll",
                Description = "Payroll Generated for Employee ID : " + payroll.EmployeeId,
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // DETAILS
        public IActionResult Details(int id)
        {
            var payroll = _context.Payrolls
                .Include(x => x.Employee)
                .FirstOrDefault(x => x.PayrollId == id);

            if (payroll == null)
                return NotFound();

            return View(payroll);
        }

        // EDIT GET
        [Authorize(Roles = "Admin,HR")]
        public IActionResult Edit(int id)
        {
            var payroll = _context.Payrolls.Find(id);

            if (payroll == null)
                return NotFound();

            ViewBag.Employees =
                new SelectList(_context.Employees,
                "EmployeeId",
                "FirstName",
                payroll.EmployeeId);

            return View(payroll);
        }

        // EDIT POST
        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Payroll payroll)
        {
            payroll.GrossSalary =
            payroll.BasicSalary +
            payroll.HRA +
            payroll.DA;

            payroll.PF = payroll.BasicSalary * 12 / 100;

            payroll.ProfessionalTax = 200;

            payroll.NetSalary =
                payroll.GrossSalary -
                payroll.PF -
                payroll.ProfessionalTax;

            _context.Payrolls.Update(payroll);
            _context.SaveChanges();

            AuditLog log = new AuditLog()
            {
                //UserName = HttpContext.Session.GetString("UserName"),
                UserName = User.Identity?.Name,
                ActionType = "Update",
                ModuleName = "Payroll",
                Description = "Payroll Updated for Employee ID : " + payroll.EmployeeId,
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // DELETE GET
        [Authorize(Roles = "Admin,HR")]
        public IActionResult Delete(int id)
        {
            var payroll = _context.Payrolls
                .Include(x => x.Employee)
                .FirstOrDefault(x => x.PayrollId == id);

            if (payroll == null)
                return NotFound();

            return View(payroll);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var payroll = _context.Payrolls.Find(id);

            if (payroll != null)
            {
                AuditLog log = new AuditLog()
                {
                    //UserName = HttpContext.Session.GetString("UserName"),
                    UserName = User.Identity?.Name,
                    ActionType = "Delete",
                    ModuleName = "Payroll",
                    Description = "Payroll Deleted for Employee ID : " + payroll.EmployeeId,
                    ActionDate = DateTime.Now
                };

                _context.AuditLogs.Add(log);

                _context.Payrolls.Remove(payroll);

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        // PAYSLIP
        [Authorize]
        public IActionResult Payslip(int id)
        {
            var payroll = _context.Payrolls
        .Include(x => x.Employee)
        .FirstOrDefault(x => x.PayrollId == id);

            if (payroll == null)
                return NotFound();

            if (!User.IsInRole("Admin") &&
                !User.IsInRole("HR"))
            {
                var employeeIdClaim =
                    User.FindFirstValue(CustomClaimTypes.EmployeeId);

                if (!int.TryParse(employeeIdClaim, out int employeeId))
                {
                    return Challenge();
                }

                if (payroll.EmployeeId != employeeId)
                {
                    return Forbid();
                }
            }

            return View(payroll);
        }
    }
}
