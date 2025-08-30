using TestMaster.Models;

namespace TestMaster.Models
{
    // Lớp này dùng để hiển thị thông tin lượt giao bài kèm theo trạng thái
    public class AssignmentViewModel
    {
        public TestAssignment Assignment { get; set; }
        public string Status { get; set; }
    }
}
