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
    [Authorize]
    public class LeaveController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public LeaveController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetEmployeeId()
        {
            var employeeIdClaim =
                User.FindFirst(CustomClaimTypes.EmployeeId)?.Value;

            if (int.TryParse(employeeIdClaim, out int employeeId))
            {
                return employeeId;
            }

            return 0;
        }

        // LIST
        public IActionResult Index()
        {


            if (User.IsInRole("Admin") || User.IsInRole("HR"))
            {
                var allLeaves = _context.LeaveRequests
                    .Include(x => x.Employee)
                    .Include(x => x.LeaveType)
                    .OrderByDescending(x => x.AppliedDate)
                    .ToList();

                return View(allLeaves);
            }

            int employeeId =GetEmployeeId();
            // HttpContext.Session.GetInt32("EmployeeId") ?? 0;

            var myLeaves = _context.LeaveRequests
                .Include(x => x.Employee)
                .Include(x => x.LeaveType)
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.AppliedDate)
                .ToList();

            return View(myLeaves);

        }

        // CREATE GET

        public IActionResult Create()
        {
            int employeeId = GetEmployeeId();
            //HttpContext.Session.GetInt32("EmployeeId") ?? 0;

            var employee = _context.Employees
                .FirstOrDefault(x => x.EmployeeId == employeeId);

            if (employee == null)
            {
                ViewBag.Message = "Employee not found.";
                return View();
            }

            ViewBag.EmployeeName = employee.FullName;

            ViewBag.LeaveTypes = new SelectList(
                _context.LeaveTypes,
                "LeaveTypeId",
                "LeaveName");

            LeaveRequest leave = new LeaveRequest
            {
                EmployeeId = employeeId,
                
            };

            return View(leave);
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LeaveRequest leave)
        {
            leave.EmployeeId = GetEmployeeId();
            //HttpContext.Session.GetInt32("EmployeeId") ?? 0;

            var employee = _context.Employees
                .FirstOrDefault(x => x.EmployeeId == leave.EmployeeId);

            if (employee == null)
            {
                ViewBag.Message = "Employee not found.";
                return View(leave);
            }

            ViewBag.EmployeeName = employee.FullName;

            ViewBag.LeaveTypes = new SelectList(
                _context.LeaveTypes,
                "LeaveTypeId",
                "LeaveName",
                leave.LeaveTypeId);

            if (leave.FromDate < DateTime.Today)
            {
                ViewBag.Message =
                    "Past dates are not allowed.";

                return View(leave);
            }

            if (leave.ToDate < leave.FromDate)
            {
                ViewBag.Message =
                    "To Date cannot be less than From Date.";

                return View(leave);
            }
            if (leave.FromDate == DateTime.Today)
            {
                ViewBag.Message = "Leave must be applied at least one day before the leave date.";
                return View(leave);
            }

            var leaveType = _context.LeaveTypes
                .FirstOrDefault(x => x.LeaveTypeId == leave.LeaveTypeId);

            if (leaveType == null)
            {
                ViewBag.Message = "Please select Leave Type.";
                return View(leave);
            }

            string gender = employee.Gender?.Trim() ?? "";

            if (leaveType.LeaveName == "Maternity Leave"
                && gender != "Female")
            {
                ViewBag.Message =
                    "Only Female Employees are eligible for Maternity Leave.";

                return View(leave);
            }

            if (leaveType.LeaveName == "Paternity Leave"
                && gender != "Male")
            {
                ViewBag.Message =
                    "Only Male Employees are eligible for Paternity Leave.";

                return View(leave);
            }

            if (!leave.FromDate.HasValue)
            {
                ViewBag.Message = "Please select From Date.";
                return View(leave);
            }


            if (!leave.ToDate.HasValue)
            {
                ViewBag.Message = "Please select To Date.";
                return View(leave);
            }

            // ADD THIS BLOCK HERE
            bool leaveAlreadyExists = _context.LeaveRequests.Any(x =>
                x.EmployeeId == leave.EmployeeId &&
                x.FromDate <= leave.ToDate &&
                x.ToDate >= leave.FromDate);

            if (leaveAlreadyExists)
            {
                ViewBag.Message = "You have already applied leave for the selected dates.";
                return View(leave);
            }

            leave.NoOfDays =
                (leave.ToDate.Value - leave.FromDate.Value).Days + 1;

            leave.Status = "Pending";
            leave.AppliedDate = DateTime.Now;

            _context.LeaveRequests.Add(leave);
            _context.SaveChanges();
           AuditLog log = new AuditLog()
            {
                //UserName = HttpContext.Session.GetString("UserName"),
                UserName = User.Identity?.Name,
                ActionType = "Create",
                ModuleName = "Leave",
                Description = "Applied Leave Request ID : " + leave.LeaveRequestId,
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            _context.SaveChanges();

            TempData["Success"] =
                "Leave Request Submitted Successfully";

            return RedirectToAction(nameof(Index));

        }

        // DETAILS
        public IActionResult Details(int id)
        {
            var leave = _context.LeaveRequests
                .Include(x => x.Employee)
                .Include(x => x.LeaveType)
                .FirstOrDefault(x => x.LeaveRequestId == id);

            if (leave == null)
                return NotFound();

            return View(leave);
        }

        // EDIT GET
        [Authorize(Roles="HR,Admin")]
        public IActionResult Edit(int id)
        {

            var leave = _context.LeaveRequests
                .Include(x => x.Employee)
                .FirstOrDefault(x => x.LeaveRequestId == id);

            if (leave == null)
                return NotFound();
            // Real-time validation
            if (leave.Status == "Approved" || leave.Status == "Rejected")
            {
                TempData["Error"] = "Approved or Rejected leave cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.EmployeeName =
                leave.Employee?.FullName ?? "";

            ViewBag.LeaveTypes = new SelectList(
                _context.LeaveTypes,
                "LeaveTypeId",
                "LeaveName",
                leave.LeaveTypeId);

            return View(leave);

           
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,Admin")]
        public IActionResult Edit(LeaveRequest leave)
        {

            var existingLeave = _context.LeaveRequests
        .Include(x => x.Employee)
        .FirstOrDefault(x => x.LeaveRequestId == leave.LeaveRequestId);

            if (existingLeave == null)
                return NotFound();

            ViewBag.EmployeeName = existingLeave.Employee?.FullName;

            ViewBag.LeaveTypes = new SelectList(
                _context.LeaveTypes,
                "LeaveTypeId",
                "LeaveName",
                leave.LeaveTypeId);

            // Real-time validation
            if (existingLeave.Status != "Pending")
            {
                TempData["Error"] = "Approved or Rejected leave cannot be edited.";
                return RedirectToAction(nameof(Index));
            }
           
            // From Date Required
            if (!leave.FromDate.HasValue)
            {
                ViewBag.Message = "Please select From Date.";
                return View(leave);
            }

            // To Date Required
            if (!leave.ToDate.HasValue)
            {
                ViewBag.Message = "Please select To Date.";
                return View(leave);
            }

            // Past Date
            if (leave.FromDate.Value.Date < DateTime.Today)
            {
                ViewBag.Message = "Past dates are not allowed.";
                return View(leave);
            }

            // Apply one day before
            if (leave.FromDate.Value.Date <= DateTime.Today)
            {
                ViewBag.Message =
                    "Leave must be applied at least one day before the leave date.";

                return View(leave);
            }

            // To Date Validation
            if (leave.ToDate.Value.Date < leave.FromDate.Value.Date)
            {
                ViewBag.Message =
                    "To Date cannot be less than From Date.";

                return View(leave);
            }

            // Overlap Validation
            bool leaveExists = _context.LeaveRequests.Any(x =>
                x.EmployeeId == existingLeave.EmployeeId &&
                x.LeaveRequestId != leave.LeaveRequestId &&
                x.Status != "Rejected" &&
                x.FromDate <= leave.ToDate &&
                x.ToDate >= leave.FromDate);

            if (leaveExists)
            {
                ViewBag.Message =
                    "Leave already exists for selected dates.";

                return View(leave);
            }

            // Leave Type
            var leaveType = _context.LeaveTypes
                .FirstOrDefault(x => x.LeaveTypeId == leave.LeaveTypeId);

            if (leaveType == null)
            {
                ViewBag.Message = "Invalid Leave Type.";
                return View(leave);
            }

            string gender = existingLeave.Employee?.Gender?.Trim() ?? "";

            if (leaveType.LeaveName == "Maternity Leave" &&
                gender != "Female")
            {
                ViewBag.Message =
                    "Only Female Employees are eligible for Maternity Leave.";

                return View(leave);
            }

            if (leaveType.LeaveName == "Paternity Leave" &&
                gender != "Male")
            {
                ViewBag.Message =
                    "Only Male Employees are eligible for Paternity Leave.";

                return View(leave);
            }

            existingLeave.LeaveTypeId = leave.LeaveTypeId;
            existingLeave.FromDate = leave.FromDate;
            existingLeave.ToDate = leave.ToDate;
            existingLeave.Reason = leave.Reason;
            existingLeave.NoOfDays =
                (leave.ToDate.Value.Date - leave.FromDate.Value.Date).Days + 1;

            _context.SaveChanges();
            AuditLog log = new AuditLog()
            {
                //UserName = HttpContext.Session.GetString("UserName"),
                UserName = User.Identity?.Name,
                ActionType = "Edit",
                ModuleName = "Leave",
                Description = "Updated Leave Request ID : " + existingLeave.LeaveRequestId,
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            _context.SaveChanges();

            TempData["Success"] =
                "Leave updated successfully.";

            return RedirectToAction(nameof(Index));

        }

        // DELETE GET
        [Authorize(Roles="Admin")]
        public IActionResult Delete(int id)
        {
            var leave = _context.LeaveRequests
                .Include(x => x.Employee)
                .Include(x => x.LeaveType)
                .FirstOrDefault(x => x.LeaveRequestId == id);

            if (leave == null)
                return NotFound();

            return View(leave);
        }
        [Authorize(Roles="Admin")]
        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {

             var leave = _context.LeaveRequests.Find(id);

    if (leave == null)
        return NotFound();

    _context.LeaveRequests.Remove(leave);

    AuditLog log = new AuditLog()
    {
        //UserName = HttpContext.Session.GetString("UserName"),
        UserName = User.Identity?.Name,
        ActionType = "Delete",
        ModuleName = "Leave",
        Description = "Deleted Leave Request ID : " + leave.LeaveRequestId,
        ActionDate = DateTime.Now
    };

    _context.AuditLogs.Add(log);

    _context.SaveChanges();

    TempData["Success"] = "Leave Deleted Successfully.";

    return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,Admin")]
        public IActionResult Approve(int id)
        {

            var leave = _context.LeaveRequests
        .FirstOrDefault(x => x.LeaveRequestId == id);

            if (leave == null)
                return NotFound();

            if (leave.Status != "Pending")
            {
                TempData["Error"] = "Only Pending leave can be approved.";
                return RedirectToAction(nameof(Index));
            }

            leave.Status = "Approved";

            _context.LeaveRequests.Update(leave);

            AuditLog log = new AuditLog()
            {
                //UserName = HttpContext.Session.GetString("UserName"),
                UserName = User.Identity?.Name,
                ActionType = "Approve",
                ModuleName = "Leave",
                Description = "Approved Leave Request ID : " + leave.LeaveRequestId,
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(log);

            _context.SaveChanges();

            TempData["Success"] = "Leave Approved Successfully.";

            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,Admin")]
        public IActionResult Reject(int id)
        {
            var leave = _context.LeaveRequests
        .FirstOrDefault(x => x.LeaveRequestId == id);

            if (leave == null)
                return NotFound();

            if (leave.Status != "Pending")
            {
                TempData["Error"] = "Only Pending leave can be rejected.";
                return RedirectToAction(nameof(Index));
            }

            leave.Status = "Rejected";

            _context.LeaveRequests.Update(leave);

            AuditLog log = new AuditLog()
            {
                //UserName = HttpContext.Session.GetString("UserName"),
                UserName = User.Identity?.Name,
                ActionType = "Reject",
                ModuleName = "Leave",
                Description = "Rejected Leave Request ID : " + leave.LeaveRequestId,
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(log);

            _context.SaveChanges();

            TempData["Success"] = "Leave Rejected Successfully.";

            return RedirectToAction(nameof(Index));

        }
    }
}
