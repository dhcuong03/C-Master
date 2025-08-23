using System.Collections.Generic;
using TestMaster.Models;

namespace TestMaster.ViewModels
{
    public class ProfileViewEmployee
    {
        // Dùng để hiển thị thông tin cá nhân
        public User UserProfile { get; set; }

        // Dùng để hiển thị danh sách các bài thi đã làm
        public List<UserTestSession> TestHistory { get; set; }
    }
}
