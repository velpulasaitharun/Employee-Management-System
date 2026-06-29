using System.ComponentModel.DataAnnotations;

namespace HRMS.ViewModels.Auth
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username")]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [StringLength(200, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }
}
