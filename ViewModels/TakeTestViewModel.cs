using System.Collections.Generic;
using TestMaster.Models;

namespace TestMaster.ViewModels
{
    // Lớp này dùng để hiển thị bài test và nhận câu trả lời từ nhân viên
    public class TakeTestViewModel
    {
        public int SessionId { get; set; }
        public Test Test { get; set; }
        public List<UserAnswerInput> UserAnswers { get; set; } = new List<UserAnswerInput>();
        public double TimeRemainingInSeconds { get; set; }
    }

    // Lớp này đại diện cho một câu trả lời mà người dùng gửi lên
    public class UserAnswerInput
    {
        // === TRƯỜNG MỚI ĐÃ ĐƯỢC BỔ SUNG VÀO ĐÂY ===
        public int SessionId { get; set; }
        public int QuestionId { get; set; }
        public int? ChosenOptionId { get; set; } // Dùng cho câu trắc nghiệm
        public string? AnswerText { get; set; }   // Dùng cho câu tự luận
    }
}
