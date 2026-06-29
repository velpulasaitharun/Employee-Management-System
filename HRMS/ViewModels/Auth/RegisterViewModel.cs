using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.ViewModels.Auth
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Employee")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select an employee.")]
        public int EmployeeId { get; set; }

        [Required]
        [Display(Name = "Username")]
        [StringLength(100, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(200, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        [RegularExpression("Admin|HR|Employee", ErrorMessage = "Invalid role selected.")]
        public string RoleName { get; set; } = "Employee";

        public IEnumerable<SelectListItem> Employees { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
