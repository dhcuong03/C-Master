using System.Collections.Generic;
using TestMaster.Models;

namespace TestMaster.ViewModels
{
    // Lớp mới để xử lý dữ liệu từ form chấm bài cho mỗi câu trả lời
    public class GradedAnswerInput
    {
        public int UserAnswerId { get; set; }
        public bool IsCorrect { get; set; } // True nếu được đánh dấu là "Đúng"
        public string? GraderNotes { get; set; }
    }

    public class GradingViewModel
    {
        public UserTestSession Session { get; set; }
        public List<GradedAnswerInput> GradedAnswers { get; set; }

        // Dùng để hiển thị số điểm cho mỗi câu tự luận đúng
        public decimal PointsPerEssayQuestion { get; set; }

        public GradingViewModel()
        {
            Session = new UserTestSession();
            GradedAnswers = new List<GradedAnswerInput>();
        }
    }
}
