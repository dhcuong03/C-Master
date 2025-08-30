using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestMaster.Models;
using TestMaster.ViewModels;

namespace TestMaster.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class ReportsController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public ReportsController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel // Tái sử dụng ViewModel từ Dashboard
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
                .Where(u => u.DepartmentId != null && u.Department != null)
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
                .Where(u => u.LevelId != null && u.Level != null)
                .GroupBy(u => u.Level.LevelName)
                .Select(g => new { LevelName = g.Key, Count = g.Count() })
                .OrderBy(x => x.LevelName)
                .ToListAsync();

            foreach (var item in employeesByLevel)
            {
                viewModel.LevelChartLabels.Add(item.LevelName);
                viewModel.LevelChartData.Add(item.Count);
            }

            return View(viewModel);
        }
    }
}
