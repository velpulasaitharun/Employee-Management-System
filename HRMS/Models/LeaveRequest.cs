using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HRMS.Models
{
    public class LeaveRequest
    {
        [Key]
        public int LeaveRequestId { get; set; }

        public int EmployeeId { get; set; }

        public int LeaveTypeId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        public int NoOfDays { get; set; }

        public string? Reason { get; set; }

        public string? Status { get; set; }

        public DateTime AppliedDate { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        [ForeignKey("LeaveTypeId")]
        public LeaveType? LeaveType { get; set; }
    }
}
