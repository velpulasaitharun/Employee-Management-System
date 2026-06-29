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
    public class AttendanceController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult MyAttendance()
        {
            MarkAbsentEmployees();
            var employeeIdClaim =
                User.FindFirstValue(CustomClaimTypes.EmployeeId);

            if (!int.TryParse(employeeIdClaim, out int employeeId))
            {
                return Challenge();
            }

            var myAttendance = _context.Attendances
                .Include(x => x.Employee)
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.AttendanceDate)
                .ToList();
            ViewBag.IsMyAttendance = true;
            return View("Index", myAttendance);
        }

        private void MarkAbsentEmployees()
        {

            var yesterday = DateTime.Today.AddDays(-1);

            // Missed Punch Out
            var missedPunchOutRecords = _context.Attendances
                .Where(a => a.AttendanceDate.Date == yesterday &&
                            a.PunchIn != DateTime.MinValue &&
                            a.PunchOut == null)
                .ToList();

            foreach (var item in missedPunchOutRecords)
            {
                item.Status = "Missed Punch Out";
            }

            // Employees
            var employees = _context.Employees.ToList();

            foreach (var emp in employees)
            {
                // Skip if employee joined after yesterday
                if (emp.JoiningDate == null ||
                    emp.JoiningDate.Value.Date > yesterday)
                {
                    continue;
                }

                bool attendanceExists = _context.Attendances.Any(a =>
                    a.EmployeeId == emp.EmployeeId &&
                    a.AttendanceDate.Date == yesterday);

                if (!attendanceExists)
                {
                    _context.Attendances.Add(new Attendance
                    {
                        EmployeeId = emp.EmployeeId,
                        AttendanceDate = yesterday,
                        PunchIn = DateTime.MinValue,
                        PunchOut = null,
                        TotalHours = 0,
                        Status = "Absent"
                    });
                }
            }

            _context.SaveChanges();


            /*var yesterday = DateTime.Today.AddDays(-1);

            // Missed Punch Out
            var missedPunchOut = _context.Attendances
                .Where(x => x.AttendanceDate.Date == yesterday &&
                            x.PunchOut == null)
                .ToList();

            foreach (var item in missedPunchOut)
            {
                if (item.PunchIn != DateTime.MinValue)
                {
                    item.Status = "Missed Punch Out";
                }
            }

            // Absent Employees
            var employees = _context.Employees.ToList();

            foreach (var emp in employees)
            {
                bool attendanceExists = _context.Attendances.Any(a =>
                    a.EmployeeId == emp.EmployeeId &&
                    a.AttendanceDate.Date == yesterday);

                if (!attendanceExists)
                {
                    _context.Attendances.Add(new Attendance
                    {
                        EmployeeId = emp.EmployeeId,
                        AttendanceDate = yesterday,
                        Status = "Absent",
                        TotalHours = 0
                    });
                }
            }

            _context.SaveChanges();*/
        }
        // LIST
        [Authorize]
        public IActionResult Index()
        {
            MarkAbsentEmployees();
            ViewBag.IsMyAttendance = false;

            if (User.IsInRole("Admin") || User.IsInRole("HR"))
            {
                var allAttendance = _context.Attendances
                    .Include(x => x.Employee)
                    .OrderByDescending(x => x.AttendanceDate)
                    .ToList();

                return View(allAttendance);
            }

            var employeeIdClaim =
                User.FindFirstValue(CustomClaimTypes.EmployeeId);

            if (!int.TryParse(employeeIdClaim, out int employeeId))
            {
                return Challenge();
            }

            var myAttendance = _context.Attendances
                .Include(x => x.Employee)
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.AttendanceDate)
                .ToList();

            ViewBag.IsMyAttendance = true;

            return View(myAttendance);
        }

        // CREATE GET
        [Authorize(Roles = "Admin,HR")]
        public IActionResult Create()
        {
            ViewBag.Employees = new SelectList(
                _context.Employees,
                "EmployeeId",
                "FullName");

            return View();
        }

        // CREATE POST
        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Attendance attendance)
        {
            if (attendance.PunchOut != null)
            {
                attendance.TotalHours =
                Convert.ToDecimal(
                (attendance.PunchOut.Value - attendance.PunchIn).TotalHours);
            }

            _context.Attendances.Add(attendance);
            _context.SaveChanges();

            AuditLog log = new AuditLog()
            {
                UserName = HttpContext.Session.GetString("UserName"),
                ActionType = "Create",
                ModuleName = "Attendance",
                Description = "Attendance Added for Employee ID : " + attendance.EmployeeId,
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        [Authorize]
        public IActionResult PunchIn()
        {
            var employeeIdClaim =
         User.FindFirstValue(CustomClaimTypes.EmployeeId);

            if (!int.TryParse(employeeIdClaim, out int employeeId))
            {
                return Challenge();
            }

            bool alreadyPunched =
                _context.Attendances.Any(x =>
                    x.EmployeeId == employeeId &&
                    x.AttendanceDate.Date == DateTime.Today);

            if (alreadyPunched)
            {
                TempData["Error"] = "Already punched in today.";
                return RedirectToAction(nameof(Index));
            }

            Attendance attendance = new Attendance
            {
                EmployeeId = employeeId,
                AttendanceDate = DateTime.Today,
                PunchIn = DateTime.Now,
                Status = "Present",
                TotalHours = 0
            };

            _context.Attendances.Add(attendance);
            _context.SaveChanges();

            TempData["Success"] = "Punch In Successful";

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public IActionResult PunchOut(int id)
        {
            var employeeIdClaim =
         User.FindFirstValue(CustomClaimTypes.EmployeeId);

            if (!int.TryParse(employeeIdClaim, out int employeeId))
            {
                return Challenge();
            }

            var attendance = _context.Attendances
                .FirstOrDefault(x =>
                    x.EmployeeId == employeeId &&
                    x.AttendanceDate.Date == DateTime.Today);

            if (attendance == null)
            {
                TempData["Error"] = "Please Punch In First";
                return RedirectToAction(nameof(Index));
            }

            if (attendance.PunchOut != null)
            {
                TempData["Error"] = "Already Punched Out";
                return RedirectToAction(nameof(Index));
            }

            attendance.PunchOut = DateTime.Now;

            attendance.TotalHours =
                Math.Round(
                    (decimal)(attendance.PunchOut.Value -
                    attendance.PunchIn).TotalHours, 2);

            attendance.Status = "Present";

            _context.SaveChanges();

            TempData["Success"] = "Punch Out Successful";

            return RedirectToAction(nameof(Index));


        }


        // DETAILS
        public IActionResult Details(int id)
        {
            var attendance = _context.Attendances
                .Include(a => a.Employee)
                .FirstOrDefault(a => a.AttendanceId == id);

            if (attendance == null)
                return NotFound();

            return View(attendance);
        }

        // EDIT GET
        [Authorize(Roles = "Admin,HR")]
        public IActionResult Edit(int id)
        {
            var attendance = _context.Attendances
                .FirstOrDefault(a => a.AttendanceId == id);

            if (attendance == null)
                return NotFound();

            ViewBag.Employees = new SelectList(
                _context.Employees,
                "EmployeeId",
                "FullName",
                attendance.EmployeeId);

            return View(attendance);
        }

        // EDIT POST
        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Attendance attendance)
        {
            if (attendance.PunchOut != null)
            {
                attendance.TotalHours =
               Convert.ToDecimal(
               (attendance.PunchOut.Value - attendance.PunchIn).TotalHours);
            }

            _context.Attendances.Update(attendance);
            _context.SaveChanges();

            AuditLog log = new AuditLog()
            {
                UserName = HttpContext.Session.GetString("UserName"),
                ActionType = "Update",
                ModuleName = "Attendance",
                Description = "Attendance Updated for Employee ID : " + attendance.EmployeeId,
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // DELETE GET
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var attendance = _context.Attendances
                .Include(a => a.Employee)
                .FirstOrDefault(a => a.AttendanceId == id);

            if (attendance == null)
                return NotFound();

            return View(attendance);
        }

        // DELETE POST
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var attendance = _context.Attendances.Find(id);

            if (attendance != null)
            {
                AuditLog log = new AuditLog()
                {
                    UserName = HttpContext.Session.GetString("UserName"),
                    ActionType = "Delete",
                    ModuleName = "Attendance",
                    Description = "Attendance Deleted for Employee ID : " + attendance.EmployeeId,
                    ActionDate = DateTime.Now
                };

                _context.AuditLogs.Add(log);

                _context.Attendances.Remove(attendance);

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
