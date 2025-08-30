using TestMaster.Models;

namespace TestMaster.ViewModels
{
    // Lớp này chứa tất cả dữ liệu chi tiết cho trang xem kết quả
    public class TestResultViewModel
    {
        public UserTestSession Session { get; set; }

        public TestResultViewModel()
        {
            Session = new UserTestSession();
        }
    }
}
