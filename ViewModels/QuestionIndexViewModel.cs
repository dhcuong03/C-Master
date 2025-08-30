using TestMaster.Models;

namespace TestMaster.ViewModels
{
    // ViewModel này dùng để gửi dữ liệu ra trang Index của Question
    // Nó chứa đối tượng Question và một biến bool để cho biết câu hỏi đã được sử dụng hay chưa
    public class QuestionIndexViewModel
    {
        public Question Question { get; set; }
        public bool IsInUse { get; set; }
    }
}
