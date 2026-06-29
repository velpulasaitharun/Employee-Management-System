using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HRMS.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public int EmployeeId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string RoleName { get; set; } = "Employee";

        public bool IsActive { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }
    }
}
