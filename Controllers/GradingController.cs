using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestMaster.Models;
using TestMaster.ViewModels;

namespace TestMaster.Controllers
{
    [Authorize(Roles = "Admin,HR,Manager")]
    public class GradingController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public GradingController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        // GET: /Grading/Index
        public async Task<IActionResult> Index()
        {
            // === SỬA LỖI: Thêm điều kiện để lọc bỏ dữ liệu cũ không hợp lệ ===
            var submittedTests = await _context.UserTestSessions
                .Include(s => s.User)
                .Include(s => s.Test)
                .Where(s => s.User != null && s.Test != null && (s.Status == "COMPLETED" || s.Status == "GRADED"))
                .OrderByDescending(s => s.EndTime)
                .ToListAsync();

            return View(submittedTests);
        }

        // GET: /Grading/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var testSession = await _context.UserTestSessions
                .Include(s => s.User)
                .Include(s => s.Test)
                .Include(s => s.UserAnswers)
                    .ThenInclude(ua => ua.Question)
                        .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (testSession == null) return NotFound();

            return View(testSession);
        }

        // GET: /Grading/GradeSession/5
        [HttpGet]
        public async Task<IActionResult> GradeSession(int? id)
        {
            if (id == null) return NotFound();

            var session = await _context.UserTestSessions
                .Include(s => s.User)
                .Include(s => s.Test)
                    .ThenInclude(t => t.Questions)
                .Include(s => s.UserAnswers)
                    .ThenInclude(ua => ua.Question)
                        .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null) return NotFound();

            int totalQuestions = session.Test.Questions.Count;
            decimal pointsPerQuestion = (totalQuestions > 0) ? 10.0m / totalQuestions : 0;

            var viewModel = new GradingViewModel
            {
                Session = session,
                PointsPerEssayQuestion = pointsPerQuestion,
                GradedAnswers = session.UserAnswers.Select(ua => new GradedAnswerInput
                {
                    UserAnswerId = ua.UserAnswerId,
                    IsCorrect = (ua.Score.HasValue && ua.Score > 0),
                    GraderNotes = ua.GraderNotes
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: /Grading/GradeSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeSession(GradingViewModel viewModel)
        {
            var graderId = int.Parse(User.FindFirstValue("UserId"));

            var sessionToUpdate = await _context.UserTestSessions
                .Include(s => s.UserAnswers)
                    .ThenInclude(ua => ua.Question)
                .Include(s => s.Test)
                    .ThenInclude(t => t.Questions)
                .FirstOrDefaultAsync(s => s.SessionId == viewModel.Session.SessionId);

            if (sessionToUpdate == null) return NotFound();

            int totalQuestions = sessionToUpdate.Test.Questions.Count;
            decimal pointsPerQuestion = (totalQuestions > 0) ? 10.0m / totalQuestions : 0;

            // 1. Cập nhật điểm cho các câu tự luận
            foreach (var gradedInput in viewModel.GradedAnswers)
            {
                var originalAnswer = sessionToUpdate.UserAnswers
                    .FirstOrDefault(ua => ua.UserAnswerId == gradedInput.UserAnswerId);

                if (originalAnswer != null && originalAnswer.Question.QuestionType == "ESSAY")
                {
                    originalAnswer.Score = gradedInput.IsCorrect ? pointsPerQuestion : 0;
                    originalAnswer.GraderNotes = gradedInput.GraderNotes;
                    originalAnswer.GradedBy = graderId;
                    originalAnswer.GradedAt = System.DateTime.Now;
                }
            }

            // 2. Đếm lại tổng số câu trả lời đúng
            int correctAnswersCount = sessionToUpdate.UserAnswers.Count(ua => ua.Score.HasValue && ua.Score > 0);

            // 3. Tính điểm cuối cùng
            decimal finalScore = (totalQuestions > 0)
                ? ((decimal)correctAnswersCount / totalQuestions) * 10
                : 0;

            // 4. Cập nhật phiên làm bài
            sessionToUpdate.FinalScore = Math.Round(finalScore, 2);
            sessionToUpdate.IsPassed = sessionToUpdate.FinalScore >= sessionToUpdate.Test.PassingScore;
            sessionToUpdate.Status = "GRADED";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Chấm bài thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Grading/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var testSession = await _context.UserTestSessions
                .Include(s => s.User)
                .Include(s => s.Test)
                .FirstOrDefaultAsync(m => m.SessionId == id);

            if (testSession == null) return NotFound();

            return View(testSession);
        }

        // POST: /Grading/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var testSession = await _context.UserTestSessions.FindAsync(id);
            if (testSession != null)
            {
                _context.UserTestSessions.Remove(testSession);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa thành công kết quả bài làm.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
