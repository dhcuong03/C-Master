using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestMaster.Models;
using TestMaster.ViewModels;
using System;

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

        #region Unchanged Actions
        // GET: /Grading/Index
        public async Task<IActionResult> Index()
        {
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
                GradedAnswers = session.UserAnswers.Where(ua => ua.Question.QuestionType == "ESSAY")
                .Select(ua => new GradedAnswerInput
                {
                    UserAnswerId = ua.UserAnswerId,
                    IsCorrect = (ua.Score.HasValue && ua.Score > 0),
                    GraderNotes = ua.GraderNotes
                }).ToList()
            };
            return View(viewModel);
        }
        #endregion

        // POST: /Grading/GradeSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeSession(GradingViewModel viewModel)
        {
            // === SỬA LỖI: Hoàn nguyên về "UserId" để lấy ID người chấm bài ===
            var graderIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(graderIdString, out var graderId))
            {
                // Nếu không tìm thấy UserId, trả về lỗi không có quyền truy cập
                return Unauthorized();
            }

            var sessionToUpdate = await _context.UserTestSessions
                .Include(s => s.UserAnswers)
                    .ThenInclude(ua => ua.Question)
                .Include(s => s.Test)
                    .ThenInclude(t => t.Questions)
                .FirstOrDefaultAsync(s => s.SessionId == viewModel.Session.SessionId);

            if (sessionToUpdate == null) return NotFound();

            // === LOGIC CHẤM BÀI MỚI - AN TOÀN VÀ CHÍNH XÁC HƠN ===

            // 1. Lấy điểm của các câu trắc nghiệm đã được chấm tự động trước đó.
            //    Điểm này đã được tính khi người dùng nộp bài.
            decimal autoGradedScore = sessionToUpdate.UserAnswers
                .Where(ua => ua.Question.QuestionType != "ESSAY" && ua.Score.HasValue)
                .Sum(ua => ua.Score.Value);

            // 2. Bắt đầu tổng điểm bằng điểm đã có.
            decimal totalScore = autoGradedScore;
            int totalQuestions = sessionToUpdate.Test.Questions.Count;
            decimal pointsPerQuestion = (totalQuestions > 0) ? 10.0m / totalQuestions : 0;

            // 3. Chỉ duyệt qua và cập nhật điểm cho các câu tự luận (ESSAY)
            foreach (var gradedInput in viewModel.GradedAnswers)
            {
                var originalAnswer = sessionToUpdate.UserAnswers
                    .FirstOrDefault(ua => ua.UserAnswerId == gradedInput.UserAnswerId);

                // Đảm bảo chỉ cập nhật câu tự luận
                if (originalAnswer != null && originalAnswer.Question.QuestionType == "ESSAY")
                {
                    originalAnswer.Score = gradedInput.IsCorrect ? pointsPerQuestion : 0;
                    originalAnswer.GraderNotes = gradedInput.GraderNotes;
                    originalAnswer.GradedBy = graderId;
                    originalAnswer.GradedAt = DateTime.Now;

                    // 4. Cộng điểm của câu tự luận vừa chấm vào tổng điểm
                    totalScore += originalAnswer.Score.Value;
                }
            }

            // 5. Cập nhật phiên làm bài với tổng điểm cuối cùng
            sessionToUpdate.FinalScore = Math.Round(totalScore, 2);
            sessionToUpdate.IsPassed = sessionToUpdate.FinalScore >= (sessionToUpdate.Test?.PassingScore ?? 0);
            sessionToUpdate.Status = "GRADED"; // Chuyển trạng thái sang "Đã chấm"

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Chấm bài thành công!";
            return RedirectToAction(nameof(Index));
        }

        #region Unchanged Actions 2
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
        #endregion
    }
}
