using System.ComponentModel.DataAnnotations;

namespace TestMaster.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [Display(Name = "Địa chỉ Email")]
        public string Email { get; set; }
    }
}
