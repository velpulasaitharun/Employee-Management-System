using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class EmployeeController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {

            var employees = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
       
                .ToList();

            return View(employees);
        }

        public IActionResult Create()
        {
            ViewBag.Departments =
                new SelectList(_context.Departments,
                "DepartmentId",
                "DepartmentName");

            ViewBag.Designations =
                new SelectList(_context.Designations,
                "DesignationId",
                "DesignationName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Employee employee)
        {
            if (_context.Employees.Any(x => x.Email == employee.Email))
            {
                ModelState.AddModelError("Email", "Email already exists");
            }

            if (_context.Employees.Any(x => x.PhoneNo == employee.PhoneNo))
            {
                ModelState.AddModelError("PhoneNo", "Phone Number already exists");
            }

            if (employee.JoiningDate > DateTime.Today)
            {
                ModelState.AddModelError("JoiningDate",
                    "Joining Date cannot be a future date");
            }


            employee.IsActive = true;
            employee.CreatedDate = DateTime.Now;

            var lastEmployee = _context.Employees
            .OrderByDescending(x => x.EmployeeCode)
            .FirstOrDefault();

            if (lastEmployee == null)
            {
                employee.EmployeeCode = "EMP001";
            }
            else
            {
                int codeNumber = int.Parse(
                    lastEmployee.EmployeeCode.Replace("EMP", "")
                );

                employee.EmployeeCode =
                    "EMP" + (codeNumber + 1).ToString("D3");
            }

            /*var lastEmployee = _context.Employees
                .OrderByDescending(x => x.EmployeeId)
                .FirstOrDefault();

            if (lastEmployee == null)
            {
                employee.EmployeeCode = "EMP001";
            }
            else
            {
                int nextId = lastEmployee.EmployeeId + 1;
                employee.EmployeeCode = "EMP" + nextId.ToString("D3");
            }*/

            if (ModelState.IsValid)
            {
                _context.Employees.Add(employee);
                _context.SaveChanges();

                AuditLog log = new AuditLog()
                {
                    UserName = HttpContext.Session.GetString("UserName"),
                    ActionType = "Create",
                    ModuleName = "Employee",
                    Description = "Employee Added : " + employee.FirstName,
                    ActionDate = DateTime.Now
                };

                _context.AuditLogs.Add(log);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = new SelectList(
                _context.Departments,
                "DepartmentId",
                "DepartmentName",
                employee.DepartmentId);

            ViewBag.Designations = new SelectList(
                _context.Designations,
                "DesignationId",
                "DesignationName",
                employee.DesignationId);

            return View(employee);
        }

        public IActionResult Details(int id)
        {
            var employee = _context.Employees
                .Include(x => x.Department)
                .Include(x => x.Designation)
                .FirstOrDefault(x => x.EmployeeId == id);

            if (employee == null)
                return NotFound();

            return View(employee);
        }

        public IActionResult Edit(int id)
        {
            var employee = _context.Employees.Find(id);

            if (employee == null)
                return NotFound();

            ViewBag.Departments =
                new SelectList(_context.Departments,
                "DepartmentId",
                "DepartmentName");

            ViewBag.Designations =
                new SelectList(_context.Designations,
                "DesignationId",
                "DesignationName");

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Employee employee)
        {
            if (_context.Employees.Any(x =>
         x.Email == employee.Email &&
         x.EmployeeId != employee.EmployeeId))
            {
                ModelState.AddModelError("Email", "Email already exists");
            }

            if (_context.Employees.Any(x =>
                x.PhoneNo == employee.PhoneNo &&
                x.EmployeeId != employee.EmployeeId))
            {
                ModelState.AddModelError("PhoneNo",
                    "Phone Number already exists");
            }

            if (employee.JoiningDate > DateTime.Today)
            {
                ModelState.AddModelError("JoiningDate",
                    "Joining Date cannot be a future date");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = new SelectList(
                    _context.Departments,
                    "DepartmentId",
                    "DepartmentName",
                    employee.DepartmentId);

                ViewBag.Designations = new SelectList(
                    _context.Designations,
                    "DesignationId",
                    "DesignationName",
                    employee.DesignationId);

                return View(employee);
            }

            var emp = _context.Employees
                .FirstOrDefault(x => x.EmployeeId == employee.EmployeeId);

            if (emp == null)
                return NotFound();

            emp.EmployeeCode = employee.EmployeeCode;
            emp.FirstName = employee.FirstName;
            emp.LastName = employee.LastName;
            emp.Gender = employee.Gender;
            emp.DOB = employee.DOB;
            emp.Email = employee.Email;
            emp.PhoneNo = employee.PhoneNo;
            emp.Address = employee.Address;
            emp.JoiningDate = employee.JoiningDate;
            emp.Salary = employee.Salary;
            emp.DepartmentId = employee.DepartmentId;
            emp.DesignationId = employee.DesignationId;

            _context.SaveChanges();

            AuditLog log = new AuditLog()
            {
                UserName = HttpContext.Session.GetString("UserName"),
                ActionType = "Update",
                ModuleName = "Employee",
                Description = "Updated Employee : " + emp.FirstName,
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));

        }

        public IActionResult Delete(int id)
        {
            var employee = _context.Employees.Find(id);

            if (employee == null)
                return NotFound();

            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var employee = _context.Employees.Find(id);

            if (employee != null)
            {
                AuditLog log = new AuditLog()
                {
                    UserName = HttpContext.Session.GetString("UserName"),
                    ActionType = "Delete",
                    ModuleName = "Employee",
                    Description = "Deleted Employee : " + employee.FirstName,
                    ActionDate = DateTime.Now
                };

                _context.AuditLogs.Add(log);

                _context.Employees.Remove(employee);

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
