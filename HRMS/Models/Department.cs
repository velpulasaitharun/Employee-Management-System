using System.ComponentModel.DataAnnotations;
namespace HRMS.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        public string? DepartmentName { get; set; }
        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public ICollection<Employee>? Employees { get; set; }
    }
}
