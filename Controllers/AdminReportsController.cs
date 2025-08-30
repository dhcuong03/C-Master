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
    [Authorize(Roles = "Admin")]
    public class AdminReportsController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public AdminReportsController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        // GET: /AdminReports/Index
        public async Task<IActionResult> Index()
        {
            ViewData["TotalUsers"] = await _context.Users.CountAsync();
            ViewData["TotalTests"] = await _context.Tests.CountAsync();
            ViewData["TotalDepartments"] = await _context.Departments.CountAsync();
            return View();
        }

        #region Report Actions

        // GET: /AdminReports/ReportByRole
        public async Task<IActionResult> ReportByRole()
        {
            var reportData = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role != null && u.Role.RoleName != "Admin")
                .GroupBy(u => u.Role.RoleName)
                .Select(g => new ReportByRoleViewModel
                {
                    RoleName = g.Key,
                    UserCount = g.Count()
                })
                .OrderByDescending(r => r.UserCount)
                .ToListAsync();

            return View(reportData);
        }

        // GET: /AdminReports/ReportByLevel
        // === LOGIC TÍNH TOÁN ĐẦY ĐỦ ĐÃ ĐƯỢC THÊM VÀO ĐÂY ===
        public async Task<IActionResult> ReportByLevel()
        {
            var reportData = await _context.Levels
                .Include(l => l.Users)
                    .ThenInclude(u => u.Role)
                .Include(l => l.Users)
                    .ThenInclude(u => u.UserTestSessions)
                .Where(l => l.Users.Any(u => u.Role.RoleName != "Admin"))
                .Select(l => new ReportByLevelViewModel
                {
                    LevelName = l.LevelName,
                    UserCount = l.Users.Count(u => u.Role.RoleName != "Admin"),

                    // Tính điểm kỹ năng trung bình từ FinalScore
                    AverageSkillScore = l.Users
                                         .Where(u => u.Role.RoleName != "Admin" && u.UserTestSessions.Any(s => s.FinalScore.HasValue))
                                         .SelectMany(u => u.UserTestSessions)
                                         .DefaultIfEmpty()
                                         .Average(ts => ts == null ? 0 : (double)(ts.FinalScore ?? 0)),

                    // Lấy thông tin phân bổ vai trò
                    RoleDistribution = l.Users
                                        .Where(u => u.Role != null && u.Role.RoleName != "Admin")
                                        .GroupBy(u => u.Role.RoleName)
                                        .Select(g => new RoleDistributionViewModel
                                        {
                                            RoleName = g.Key,
                                            UserCount = g.Count()
                                        })
                                        .OrderByDescending(rd => rd.UserCount)
                                        .ToList()
                })
                .ToListAsync();

            // Sắp xếp kết quả theo thứ tự logic (Junior -> Middle -> Senior)
            var levelOrder = new Dictionary<string, int> { { "Junior", 1 }, { "Middle", 2 }, { "Senior", 3 }, { "Expert", 4 } };
            var sortedReportData = reportData
                                    .OrderBy(r => levelOrder.ContainsKey(r.LevelName) ? levelOrder[r.LevelName] : 99)
                                    .ToList();

            return View(sortedReportData);
        }

        // GET: /AdminReports/IndividualReport
        [HttpGet]
        public async Task<IActionResult> IndividualReport()
        {
            var viewModel = new IndividualReportViewModel
            {
                AllUsers = await _context.Users
                    .Where(u => u.Role.RoleName != "Admin")
                    .OrderBy(u => u.FullName)
                    .ToListAsync()
            };
            return View(viewModel);
        }

        // POST: /AdminReports/IndividualReport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IndividualReport(int selectedUserId)
        {
            var allUsers = await _context.Users
                                .Where(u => u.Role.RoleName != "Admin")
                                .OrderBy(u => u.FullName)
                                .ToListAsync();

            var selectedUser = allUsers.FirstOrDefault(u => u.UserId == selectedUserId);

            var viewModel = new IndividualReportViewModel
            {
                AllUsers = allUsers,
                SelectedUser = selectedUser
            };

            if (selectedUser != null)
            {
                // Dữ liệu giả lập cho báo cáo cá nhân
                viewModel.SkillScores = new System.Collections.Generic.List<SkillScoreViewModel>
                {
                    new SkillScoreViewModel { SkillName = "Lập trình C#", AverageScore = 8.5 },
                    new SkillScoreViewModel { SkillName = "ASP.NET Core", AverageScore = 9.1 },
                    new SkillScoreViewModel { SkillName = "SQL & Databases", AverageScore = 7.6 }
                };
            }

            return View(viewModel);
        }

        #endregion
    }
}
