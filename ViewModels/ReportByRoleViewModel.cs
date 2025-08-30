using System.Collections.Generic;

namespace TestMaster.ViewModels
{
    /// <summary>
    /// ViewModel chứa dữ liệu thống kê cho một vai trò cụ thể.
    /// </summary>
    public class ReportByRoleViewModel
    {
        public string RoleName { get; set; }
        public int UserCount { get; set; }
        // Chúng ta có thể thêm các thông tin khác sau này,
        // ví dụ: điểm trung bình, tỷ lệ hoàn thành bài test...
    }
}
