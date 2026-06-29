
using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Department
        public IActionResult Index()
        {
            var departments = _context.Departments.ToList();
            return View(departments);
        }

        // GET: Department/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Departments.Add(department);
                _context.SaveChanges();

                AuditLog log = new AuditLog()
                {
                    UserName = HttpContext.Session.GetString("UserName"),
                    ActionType = "Create",
                    ModuleName = "Department",
                    Description = "Department Added : " + department.DepartmentName,
                    ActionDate = DateTime.Now
                };

                _context.AuditLogs.Add(log);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(department);
        }

        // GET: Department/Edit/5
        public IActionResult Edit(int id)
        {
            var department = _context.Departments.Find(id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: Department/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Departments.Update(department);
                _context.SaveChanges();

                AuditLog log = new AuditLog()
                {
                    UserName = HttpContext.Session.GetString("UserName"),
                    ActionType = "Update",
                    ModuleName = "Department",
                    Description = "Department Updated : " + department.DepartmentName,
                    ActionDate = DateTime.Now
                };

                _context.AuditLogs.Add(log);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(department);
        }

        // GET: Department/Details/5
        public IActionResult Details(int id)
        {
            var department = _context.Departments.Find(id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: Department/Delete/5
        public IActionResult Delete(int id)
        {
            var department = _context.Departments.Find(id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: Department/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var department = _context.Departments.Find(id);

            if (department != null)
            {
                AuditLog log = new AuditLog()
                {
                    UserName = HttpContext.Session.GetString("UserName"),
                    ActionType = "Delete",
                    ModuleName = "Department",
                    Description = "Department Deleted : " + department.DepartmentName,
                    ActionDate = DateTime.Now
                };

                _context.AuditLogs.Add(log);

                _context.Departments.Remove(department);

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
