using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
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
        private readonly IMemoryCache _cache;
        private const string CompanyScoresCacheKey = "CompanyAverageScores";

        public AdminReportsController(EmployeeAssessmentContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
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

        // GET: /AdminReports/ReportByRole (Không thay đổi)
        public async Task<IActionResult> ReportByRole()
        {
            // ... (Phần này giữ nguyên)
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

        // GET: /AdminReports/ReportByLevel (Không thay đổi)
        public async Task<IActionResult> ReportByLevel()
        {
            // ... (Phần này giữ nguyên)
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
                   AverageSkillScore = l.Users
                                        .Where(u => u.Role.RoleName != "Admin" && u.UserTestSessions.Any(s => s.FinalScore.HasValue))
                                        .SelectMany(u => u.UserTestSessions)
                                        .DefaultIfEmpty()
                                        .Average(ts => ts == null ? 0 : (double)(ts.FinalScore ?? 0)),
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

            var levelOrder = new Dictionary<string, int> { { "Junior", 1 }, { "Middle", 2 }, { "Senior", 3 } };
            var sortedReportData = reportData
                                     .OrderBy(r => levelOrder.ContainsKey(r.LevelName) ? levelOrder[r.LevelName] : 99)
                                     .ToList();

            return View(sortedReportData);
        }

        // GET: /AdminReports/IndividualReport
        public async Task<IActionResult> IndividualReport(int? userId)
        {
            var viewModel = new IndividualReportViewModel
            {
                UserList = new SelectList(await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role != null && u.Role.RoleName == "Employee")
                    .OrderBy(u => u.FullName)
                    .ToListAsync(), "UserId", "FullName", userId),
                SelectedUserId = userId
            };

            if (userId.HasValue)
            {
                viewModel.SelectedUser = await _context.Users.FindAsync(userId.Value);
                if (viewModel.SelectedUser == null)
                {
                    return NotFound();
                }

                var userAnswers = await _context.UserAnswers
                    .Where(ua => ua.Session.UserId == userId.Value && ua.Score.HasValue && ua.Question.SkillId.HasValue)
                    .Include(ua => ua.Question)
                        .ThenInclude(q => q.Skill)
                    .ToListAsync();

                // 2. Tính điểm trung bình của nhân viên cho từng kỹ năng
                var individualSkillScores = userAnswers
                    .Where(ua => ua.Question.Skill != null)
                    .GroupBy(ua => ua.Question.Skill)
                    .Select(g => new {
                        SkillId = g.Key.SkillId,
                        // SỬA ĐỔI: Nhân với 100 để quy đổi về thang điểm 100
                        AverageScore = g.Average(ua => ua.Score.Value) * 100m
                    })
                    .ToDictionary(x => x.SkillId, x => x.AverageScore);

                if (!_cache.TryGetValue(CompanyScoresCacheKey, out Dictionary<int, decimal> companyAverageScores))
                {
                    // 4. Nếu Cache không có, truy vấn CSDL để tính toán
                    companyAverageScores = await _context.UserAnswers
                        .Where(ua => ua.Score.HasValue && ua.Question.SkillId.HasValue && ua.Question.Skill != null)
                        .GroupBy(ua => new { ua.Question.Skill.SkillId, ua.Question.Skill.SkillName })
                        .Select(g => new {
                            g.Key.SkillId,
                            // SỬA ĐỔI: Nhân với 100 để quy đổi về thang điểm 100
                            AverageScore = g.Average(ua => ua.Score.Value) * 100m
                        })
                        .ToDictionaryAsync(x => x.SkillId, x => x.AverageScore);

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromHours(4));

                    _cache.Set(CompanyScoresCacheKey, companyAverageScores, cacheEntryOptions);
                }

                var allSkillsInvolved = await _context.Skills
                    .Where(s => companyAverageScores.Keys.Contains(s.SkillId))
                    .OrderBy(s => s.SkillName)
                    .ToListAsync();

                foreach (var skill in allSkillsInvolved)
                {
                    viewModel.SkillNames.Add(skill.SkillName);

                    decimal individualScore = individualSkillScores.TryGetValue(skill.SkillId, out var iScore) ? iScore : 0;
                    viewModel.IndividualScores.Add(Math.Round(individualScore, 2));

                    decimal companyScore = companyAverageScores.TryGetValue(skill.SkillId, out var cScore) ? cScore : 0;
                    viewModel.CompanyAverageScores.Add(Math.Round(companyScore, 2));
                }
            }
            return View(viewModel);
        }

        #endregion
    }
}