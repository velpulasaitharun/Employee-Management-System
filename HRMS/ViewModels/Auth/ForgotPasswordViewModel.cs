using System.ComponentModel.DataAnnotations;

namespace HRMS.ViewModels.Auth
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [Display(Name = "Username")]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;
    }
}
