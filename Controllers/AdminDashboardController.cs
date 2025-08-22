using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TestMaster.Models;
using TestMaster.ViewModels; // <-- THÊM DÒNG NÀY

namespace TestMaster.Controllers
{
    [Authorize(Roles = "Admin,HR,Manager")]
    public class AdminDashboardController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public AdminDashboardController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Tạo một đối tượng ViewModel để chứa tất cả dữ liệu
            var viewModel = new AdminDashboardViewModel
            {
                // Lấy dữ liệu cho các thẻ thống kê
                TotalUsers = await _context.Users.CountAsync(),
                TotalTests = await _context.Tests.CountAsync(),
                TotalDepartments = await _context.Departments.CountAsync(),
                TotalQuestions = await _context.Questions.CountAsync(),
                TotalTestSessions = await _context.UserTestSessions.CountAsync()
            };

            // Lấy dữ liệu cho biểu đồ nhân viên theo phòng ban
            var employeesByDept = await _context.Users
                .Where(u => u.DepartmentId != null) // Chỉ lấy user có phòng ban
                .GroupBy(u => u.Department.DepartmentName)
                .Select(g => new { DepartmentName = g.Key, Count = g.Count() })
                .OrderBy(x => x.DepartmentName)
                .ToListAsync();

            foreach (var item in employeesByDept)
            {
                viewModel.DepartmentChartLabels.Add(item.DepartmentName);
                viewModel.DepartmentChartData.Add(item.Count);
            }

            // Lấy dữ liệu cho biểu đồ phân bổ cấp bậc
            var employeesByLevel = await _context.Users
                .Where(u => u.LevelId != null) // Chỉ lấy user có level
                .GroupBy(u => u.Level.LevelName)
                .Select(g => new { LevelName = g.Key, Count = g.Count() })
                .OrderBy(x => x.LevelName)
                .ToListAsync();

            foreach (var item in employeesByLevel)
            {
                viewModel.LevelChartLabels.Add(item.LevelName);
                viewModel.LevelChartData.Add(item.Count);
            }

            // Gửi ViewModel chứa tất cả dữ liệu sang View
            return View(viewModel);
        }
    }
}
