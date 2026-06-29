using HRMS.Data;
using HRMS.Models;
using HRMS.Services.Interfaces;

namespace HRMS.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditService> _logger;

        public AuditService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuditService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task LogAsync(
            string actionType,
            string moduleName,
            string description,
            string? userName = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                var resolvedUserName = userName
                    ?? context?.User?.Identity?.Name
                    ?? context?.Session.GetString("UserName")
                    ?? "System";

                var log = new AuditLog
                {
                    UserName = resolvedUserName,
                    ActionType = actionType,
                    ModuleName = moduleName,
                    Description = description,
                    ActionDate = DateTime.UtcNow
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write audit log entry for {ModuleName}:{ActionType}", moduleName, actionType);
            }
        }
    }
}
