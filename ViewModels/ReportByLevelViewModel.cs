using System.Collections.Generic;

namespace TestMaster.ViewModels
{
    /// <summary>
    /// Chứa thông tin phân bổ của một vai trò cụ thể trong một cấp độ.
    /// Ví dụ: "Dev: 10 người"
    /// </summary>
    public class RoleDistributionViewModel
    {
        public string RoleName { get; set; }
        public int UserCount { get; set; }
    }

    /// <summary>
    /// ViewModel chính, chứa toàn bộ dữ liệu thống kê cho một cấp độ (Level).
    /// Phiên bản này được cập nhật để khớp với AdminReportsController.
    /// </summary>
    public class ReportByLevelViewModel
    {
        public string LevelName { get; set; }

        // === LỖI ĐÃ SỬA Ở ĐÂY ===
        // Đổi tên thuộc tính trở lại thành "UserCount" để khớp với Controller
        public int UserCount { get; set; }

        // Thêm thuộc tính để chứa điểm kỹ năng trung bình
        public double AverageSkillScore { get; set; }

        // Thêm danh sách để chứa thông tin phân bổ vai trò
        public List<RoleDistributionViewModel> RoleDistribution { get; set; }

        // Constructor để khởi tạo danh sách, tránh lỗi null reference
        public ReportByLevelViewModel()
        {
            RoleDistribution = new List<RoleDistributionViewModel>();
        }
    }
}
