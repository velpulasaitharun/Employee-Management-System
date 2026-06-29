using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HRMS.Models
{
    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }

        public int EmployeeId { get; set; }

        public DateTime AttendanceDate { get; set; }

        public DateTime PunchIn { get; set; }

        public DateTime? PunchOut { get; set; }

        public decimal TotalHours { get; set; }

        public string ?Status { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee
        {
            get; set;
        }
    }
}
