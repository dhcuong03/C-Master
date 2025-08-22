using System.Collections.Generic;

namespace TestMaster.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Dữ liệu cho các thẻ thống kê
        public int TotalUsers { get; set; }
        public int TotalTests { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalTestSessions { get; set; }

        // Dữ liệu cho biểu đồ nhân viên theo phòng ban
        public List<string> DepartmentChartLabels { get; set; } = new List<string>();
        public List<int> DepartmentChartData { get; set; } = new List<int>();

        // Dữ liệu cho biểu đồ phân bổ cấp bậc
        public List<string> LevelChartLabels { get; set; } = new List<string>();
        public List<int> LevelChartData { get; set; } = new List<int>();
    }
}
