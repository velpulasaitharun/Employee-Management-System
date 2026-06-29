using System.ComponentModel.DataAnnotations;
namespace HRMS.Models
{
    public class LeaveType
    {
        [Key]
        public int LeaveTypeId { get; set; }

        public string? LeaveName { get; set; }

        public int TotalDays { get; set; }

    }
}
