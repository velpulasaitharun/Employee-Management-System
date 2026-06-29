using System.ComponentModel.DataAnnotations;
namespace HRMS.Models
{
    public class Designation
    {
        [Key]
        public int DesignationId { get; set; }

        [Required]
        public string? DesignationName { get; set; }

        public DateTime CreatedDate { get; set; }

        public ICollection<Employee>? Employees { get; set; }
    }
}
