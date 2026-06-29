using System.ComponentModel.DataAnnotations;
namespace HRMS.Models
{
    public class Holiday
    {
        [Key]
        public int HolidayId { get; set; }

        [Required]
        public string? HolidayName { get; set; }

        [Required]
        public DateTime HolidayDate { get; set; }

        public string? Description { get; set; }

        public bool IsOptional { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}