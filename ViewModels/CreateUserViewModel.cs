using System.ComponentModel.DataAnnotations;

namespace TestMaster.ViewModels
{
    /// <summary>
    /// Đây là ViewModel, một class đặc biệt dùng để định hình và xác thực dữ liệu
    /// cho một View cụ thể. Class này được thiết kế riêng cho form "Tạo người dùng mới".
    /// Nó không phải là model của cơ sở dữ liệu (User.cs) mà chỉ là một "khuôn mẫu"
    /// cho giao diện người dùng.
    /// </summary>
    public class CreateUserViewModel
    {
        // [Required]: Bắt buộc người dùng phải nhập giá trị này.
        // ErrorMessage: Thông báo sẽ hiển thị nếu người dùng không nhập.
        // [StringLength]: Quy định độ dài tối đa và tối thiểu cho chuỗi.
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")] // Kiểm tra định dạng email.
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [DataType(DataType.Password)] // Giúp trình duyệt hiển thị ô nhập dưới dạng mật khẩu (dấu *).
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string Password { get; set; }

        // [Compare("Password")]: So sánh giá trị của trường này với trường "Password".
        // Chúng phải giống hệt nhau.
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")] // Tên hiển thị trên giao diện.
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chức vụ.")]
        [Display(Name = "Chức vụ")]
        public int RoleId { get; set; } // Sẽ được dùng để lưu ID của Role được chọn từ dropdown.

        [Required(ErrorMessage = "Vui lòng chọn phòng ban.")]
        [Display(Name = "Phòng ban")]
        public int DepartmentId { get; set; } // Sẽ được dùng để lưu ID của Department được chọn từ dropdown.
    }
}
