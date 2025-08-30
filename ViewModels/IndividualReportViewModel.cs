using System.Collections.Generic;
using TestMaster.Models;

namespace TestMaster.ViewModels
{
    /// <summary>
    /// ViewModel con, chứa thông tin về một kỹ năng cụ thể và điểm số.
    /// </summary>
    public class SkillScoreViewModel
    {
        public string SkillName { get; set; }
        public double AverageScore { get; set; }
    }

    /// <summary>
    /// ViewModel chính cho trang báo cáo năng lực cá nhân.
    /// </summary>
    public class IndividualReportViewModel
    {
        // Thông tin của nhân viên được chọn để xem báo cáo
        public User SelectedUser { get; set; }

        // Danh sách tất cả nhân viên để hiển thị trong dropdown
        public IEnumerable<User> AllUsers { get; set; }

        // Danh sách các kỹ năng và điểm số của nhân viên được chọn
        public List<SkillScoreViewModel> SkillScores { get; set; }

        public IndividualReportViewModel()
        {
            SkillScores = new List<SkillScoreViewModel>();
            AllUsers = new List<User>();
        }
    }
}
