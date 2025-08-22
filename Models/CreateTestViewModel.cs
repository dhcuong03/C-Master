using System.Collections.Generic;

namespace TestMaster.Models
{
    // Lớp này dùng để hiển thị một câu hỏi kèm theo checkbox
    public class SelectableQuestion
    {
        public int QuestionId { get; set; }
        public string Content { get; set; }
        public string SkillName { get; set; }
        public string Difficulty { get; set; }
        public bool IsSelected { get; set; }
    }

    // ViewModel chính cho trang tạo Test
    public class CreateTestViewModel
    {
        public Test Test { get; set; }
        public List<SelectableQuestion> AllQuestions { get; set; }

        public CreateTestViewModel()
        {
            Test = new Test();
            AllQuestions = new List<SelectableQuestion>();
        }
    }
}
