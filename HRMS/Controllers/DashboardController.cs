using HRMS.Constants;
using HRMS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMS.Controllers
{
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Admin & HR Dashboard
            if (User.IsInRole("Admin") || User.IsInRole("HR"))
            {
                ViewBag.TotalEmployees = _context.Employees.Count();
                ViewBag.TotalDepartments = _context.Departments.Count();
                ViewBag.TotalDesignations = _context.Designations.Count();
                ViewBag.TotalLeaves = _context.LeaveRequests.Count();
                ViewBag.TotalPayrolls = _context.Payrolls.Count();

                return View();
            }
            // Employee Dashboard
            var employeeIdClaim =
                User.FindFirstValue(CustomClaimTypes.EmployeeId);

            if (!int.TryParse(employeeIdClaim, out int employeeId))
            {
                return Challenge();
            }

            ViewBag.MyLeaves =
                _context.LeaveRequests
                .Count(x => x.EmployeeId == employeeId);

            ViewBag.MyPayrolls =
                _context.Payrolls
                .Count(x => x.EmployeeId == employeeId);

            ViewBag.MyAttendance =
                _context.Attendances
                .Count(x => x.EmployeeId == employeeId);

            ViewBag.PendingLeaves =
                _context.LeaveRequests
                .Count(x => x.EmployeeId == employeeId &&
                            x.Status == "Pending");

            ViewBag.TodayAttendance =
                _context.Attendances
                .Any(x => x.EmployeeId == employeeId &&
                          x.AttendanceDate.Date == DateTime.Today);

            return View();
        }
    }
}
