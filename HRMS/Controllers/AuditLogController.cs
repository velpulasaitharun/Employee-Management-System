using HRMS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AuditLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var logs = _context.AuditLogs
                .OrderByDescending(x => x.ActionDate)
                .ToList();

            return View(logs);
        }

        public IActionResult Details(int id)
        {
            var log = _context.AuditLogs
                .FirstOrDefault(x => x.AuditLogId == id);

            if (log == null)
                return NotFound();

            return View(log);
        }
    }
}
