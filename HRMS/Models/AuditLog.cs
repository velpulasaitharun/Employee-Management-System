using System.ComponentModel.DataAnnotations;
namespace HRMS.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        public string? UserName { get; set; }

        public string? ActionType { get; set; }

        public string? ModuleName { get; set; }

        public string? Description { get; set; }

        public DateTime ActionDate { get; set; }
    }
}
