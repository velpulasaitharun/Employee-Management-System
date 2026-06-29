using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DesignationController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public DesignationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET
        public IActionResult Index()
        {
            var designations = _context.Designations.ToList();
            return View(designations);
        }

        // GET
        public IActionResult Create()
        {
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Designation designation)
        {
            if (ModelState.IsValid)
            {
                _context.Designations.Add(designation);
                _context.SaveChanges();

                AuditLog log = new AuditLog()
                {
                    UserName = HttpContext.Session.GetString("UserName"),
                    ActionType = "Create",
                    ModuleName = "Designation",
                    Description = "Designation Added : " + designation.DesignationName,
                    ActionDate = DateTime.Now
                };

                _context.AuditLogs.Add(log);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(designation);
        }

        // GET
        public IActionResult Edit(int id)
        {
            var designation = _context.Designations.Find(id);

            if (designation == null)
            {
                return NotFound();
            }

            return View(designation);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Designation designation)
        {
            if (ModelState.IsValid)
            {
                _context.Designations.Update(designation);
                _context.SaveChanges();

                AuditLog log = new AuditLog()
                {
                    UserName = HttpContext.Session.GetString("UserName"),
                    ActionType = "Update",
                    ModuleName = "Designation",
                    Description = "Designation Updated : " + designation.DesignationName,
                    ActionDate = DateTime.Now
                };

                _context.AuditLogs.Add(log);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(designation);
        }

        // GET
        public IActionResult Details(int id)
        {
            var designation = _context.Designations.Find(id);

            if (designation == null)
            {
                return NotFound();
            }

            return View(designation);
        }

        // GET
        public IActionResult Delete(int id)
        {
            var designation = _context.Designations.Find(id);

            if (designation == null)
            {
                return NotFound();
            }

            return View(designation);
        }

        // POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var designation = _context.Designations.Find(id);

            if (designation != null)
            {
                AuditLog log = new AuditLog()
                {
                    UserName = HttpContext.Session.GetString("UserName"),
                    ActionType = "Delete",
                    ModuleName = "Designation",
                    Description = "Designation Deleted : " + designation.DesignationName,
                    ActionDate = DateTime.Now
                };

                _context.AuditLogs.Add(log);

                _context.Designations.Remove(designation);

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));

        }
    }
}
