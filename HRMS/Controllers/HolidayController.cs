using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class HolidayController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public HolidayController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LIST
        public IActionResult Index()
        {
            var holidays = _context.Holidays.ToList();
            return View(holidays);
        }

        // CREATE GET
        public IActionResult Create()
        {
            return View();
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Holiday holiday)
        {
            if (ModelState.IsValid)
            {
                holiday.CreatedDate = DateTime.Now;

                _context.Holidays.Add(holiday);
                _context.SaveChanges();

                AuditLog log = new AuditLog()
                {
                    //UserName = HttpContext.Session.GetString("UserName"),
                    UserName = User.Identity?.Name,
                    ActionType = "Create",
                    ModuleName = "Holiday",
                    Description = "Holiday Added : " + holiday.HolidayName,
                    ActionDate = DateTime.Now
                };

                _context.AuditLogs.Add(log);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(holiday);
        }

        // DETAILS
        public IActionResult Details(int id)
        {
            var holiday = _context.Holidays.Find(id);

            if (holiday == null)
                return NotFound();

            return View(holiday);
        }

        // EDIT GET
        public IActionResult Edit(int id)
        {
            var holiday = _context.Holidays.Find(id);

            if (holiday == null)
                return NotFound();

            return View(holiday);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Holiday holiday)
        {
            if (ModelState.IsValid)
            {
                _context.Holidays.Update(holiday);
                _context.SaveChanges();

                AuditLog log = new AuditLog()
                {
                    //UserName = HttpContext.Session.GetString("UserName"),
                    UserName = User.Identity?.Name,
                    ActionType = "Update",
                    ModuleName = "Holiday",
                    Description = "Holiday Updated : " + holiday.HolidayName,
                    ActionDate = DateTime.Now
                };

                _context.AuditLogs.Add(log);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(holiday);
        }

        // DELETE GET
        public IActionResult Delete(int id)
        {
            var holiday = _context.Holidays.Find(id);

            if (holiday == null)
                return NotFound();

            return View(holiday);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var holiday = _context.Holidays.Find(id);

            if (holiday != null)
            {
                AuditLog log = new AuditLog()
                {
                    //UserName = HttpContext.Session.GetString("UserName"),
                    UserName = User.Identity?.Name,
                    ActionType = "Delete",
                    ModuleName = "Holiday",
                    Description = "Holiday Deleted : " + holiday.HolidayName,
                    ActionDate = DateTime.Now
                };

                _context.AuditLogs.Add(log);

                _context.Holidays.Remove(holiday);

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
