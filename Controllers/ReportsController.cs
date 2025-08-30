using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TestMaster.Models;
using TestMaster.ViewModels;

namespace TestMaster.Controllers
{
    [Authorize(Roles = "Admin,HR,Manager")]
    public class ReportsController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public ReportsController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        // GET: /Reports/IndividualReport
        // GET: /Reports/IndividualReport?userId=5
        // Action này xử lý cả việc hiển thị form và hiển thị báo cáo chi tiết
        public async Task<IActionResult> IndividualReport(int? userId)
        {
            // Luôn chuẩn bị danh sách nhân viên cho dropdown
            var viewModel = new IndividualReportViewModel
            {
                UserList = new SelectList(await _context.Users
                    .Include(u => u.Role) // Phải tải kèm Role để có thể lọc theo RoleName
                    .Where(u => u.Role != null && u.Role.RoleName == "Employee")
                    .OrderBy(u => u.FullName)
                    .ToListAsync(), "UserId", "FullName", userId),
                SelectedUserId = userId
            };

            // Nếu một nhân viên đã được chọn, tiến hành tính toán
            if (userId.HasValue)
            {
                viewModel.SelectedUser = await _context.Users.FindAsync(userId.Value);
                if (viewModel.SelectedUser == null)
                {
                    return NotFound();
                }

                // 1. Lấy tất cả câu trả lời có điểm của nhân viên được chọn
                var userAnswers = await _context.UserAnswers
                    .Where(ua => ua.Session.UserId == userId.Value && ua.Score.HasValue && ua.Question.SkillId.HasValue)
                    .Include(ua => ua.Question)
                        .ThenInclude(q => q.Skill)
                    .ToListAsync();

                // 2. Tính điểm trung bình của nhân viên cho từng kỹ năng
                var individualSkillScores = userAnswers
                    .Where(ua => ua.Question.Skill != null) // Đảm bảo Skill không bị null
                    .GroupBy(ua => ua.Question.Skill)
                    .Select(g => new {
                        SkillId = g.Key.SkillId,
                        AverageScore = g.Average(ua => ua.Score.Value)
                    })
                    .ToDictionary(x => x.SkillId, x => x.AverageScore);

                // 3. Lấy tất cả câu trả lời để tính điểm trung bình của toàn công ty
                var allAnswers = await _context.UserAnswers
                    .Where(ua => ua.Score.HasValue && ua.Question.SkillId.HasValue)
                    .Include(ua => ua.Question)
                        .ThenInclude(q => q.Skill)
                    .ToListAsync();

                // 4. Tính điểm trung bình của toàn công ty cho từng kỹ năng
                var companyAverageScores = allAnswers
                    .Where(ua => ua.Question.Skill != null) // Đảm bảo Skill không bị null
                    .GroupBy(ua => ua.Question.Skill)
                    .Select(g => new {
                        SkillId = g.Key.SkillId,
                        AverageScore = g.Average(ua => ua.Score.Value)
                    })
                    .ToDictionary(x => x.SkillId, x => x.AverageScore);

                // 5. Chuẩn bị dữ liệu cuối cùng cho biểu đồ
                var allSkillsInvolved = await _context.Skills
                    .Where(s => s.Questions.Any(q => allAnswers.Select(a => a.QuestionId).Contains(q.QuestionId)))
                    .OrderBy(s => s.SkillName)
                    .ToListAsync();

                foreach (var skill in allSkillsInvolved)
                {
                    viewModel.SkillNames.Add(skill.SkillName);

                    decimal individualScore = individualSkillScores.TryGetValue(skill.SkillId, out var iScore) ? iScore : 0;
                    viewModel.IndividualScores.Add(System.Math.Round(individualScore, 2));

                    decimal companyScore = companyAverageScores.TryGetValue(skill.SkillId, out var cScore) ? cScore : 0;
                    viewModel.CompanyAverageScores.Add(System.Math.Round(companyScore, 2));
                }
            }

            return View(viewModel);
        }
    }
}

