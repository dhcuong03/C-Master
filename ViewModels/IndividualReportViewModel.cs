// File: TestMaster/ViewModels/IndividualReportViewModel.cs

using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using TestMaster.Models;

namespace TestMaster.ViewModels
{
    /// <summary>
    /// ViewModel chính cho trang báo cáo năng lực cá nhân.
    /// Các thuộc tính không còn sử dụng đã được lược bỏ để mã nguồn gọn gàng hơn.
    /// </summary>
    public class IndividualReportViewModel
    {
        // Thông tin của nhân viên được chọn để xem báo cáo
        public User SelectedUser { get; set; }

        // Dùng cho form lựa chọn nhân viên
        public SelectList UserList { get; set; }
        public int? SelectedUserId { get; set; }

        // Dữ liệu cho biểu đồ Radar/Cột
        public List<string> SkillNames { get; set; } = new List<string>();
        public List<decimal> IndividualScores { get; set; } = new List<decimal>();
        public List<decimal> CompanyAverageScores { get; set; } = new List<decimal>();

        // Cờ để kiểm tra xem có dữ liệu để hiển thị hay không
        public bool HasData => SkillNames.Count > 0;
    }
}