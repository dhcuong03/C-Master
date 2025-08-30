using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TestMaster.Models;
using TestMaster.ViewModels;

namespace TestMaster.Controllers
{
    [Authorize(Roles = "Employee")]
    public class TestSessionController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public TestSessionController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        // ... (Các action StartTest, TakeTest, SaveAnswer không thay đổi) ...
        #region Unchanged Actions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartTest(int testId)
        {
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId)) { return Unauthorized(); }

            var existingSession = await _context.UserTestSessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.TestId == testId && s.Status == "IN_PROGRESS");

            if (existingSession != null)
            {
                return RedirectToAction("TakeTest", new { sessionId = existingSession.SessionId });
            }

            var newSession = new UserTestSession
            {
                UserId = userId,
                TestId = testId,
                StartTime = DateTime.Now,
                Status = "IN_PROGRESS"
            };

            _context.UserTestSessions.Add(newSession);
            await _context.SaveChangesAsync();

            return RedirectToAction("TakeTest", new { sessionId = newSession.SessionId });
        }

        [HttpGet]
        public async Task<IActionResult> TakeTest(int sessionId)
        {
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId)) { return Unauthorized(); }

            var session = await _context.UserTestSessions
                .Include(s => s.Test)
                    .ThenInclude(t => t.Questions)
                        .ThenInclude(q => q.AnswerOptions)
                .Include(s => s.UserAnswers)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

            if (session == null || session.Status != "IN_PROGRESS")
            {
                TempData["ErrorMessage"] = "Phiên làm bài không hợp lệ hoặc đã kết thúc.";
                return RedirectToAction("MyTests", "EmployeeDashboard");
            }

            var timeElapsed = (DateTime.Now - session.StartTime.Value).TotalSeconds;
            var timeRemaining = (session.Test.DurationMinutes * 60) - timeElapsed;
            if (timeRemaining <= 0)
            {
                TempData["InfoMessage"] = "Đã hết giờ làm bài. Bài của bạn đã được nộp tự động.";
                return await ProcessAndSubmitTest(session.SessionId, new List<UserAnswerInput>());
            }

            var viewModel = new TakeTestViewModel
            {
                SessionId = session.SessionId,
                Test = session.Test,
                TimeRemainingInSeconds = timeRemaining,
                UserAnswers = session.Test.Questions.OrderBy(q => q.QuestionId).Select(q => {
                    var savedAnswer = session.UserAnswers.FirstOrDefault(ua => ua.QuestionId == q.QuestionId);
                    return new UserAnswerInput
                    {
                        QuestionId = q.QuestionId,
                        ChosenOptionId = savedAnswer?.ChosenOptionId,
                        AnswerText = savedAnswer?.AnswerText
                    };
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveAnswer([FromBody] UserAnswerInput answerInput)
        {
            if (answerInput == null) return BadRequest();
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId)) { return Unauthorized(); }

            var session = await _context.UserTestSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SessionId == answerInput.SessionId && s.UserId == userId && s.Status == "IN_PROGRESS");

            if (session == null)
            {
                return Json(new { success = false, message = "Phiên làm bài không hợp lệ." });
            }

            var existingAnswer = await _context.UserAnswers
                .FirstOrDefaultAsync(ua => ua.SessionId == answerInput.SessionId && ua.QuestionId == answerInput.QuestionId);

            if (existingAnswer != null)
            {
                existingAnswer.ChosenOptionId = answerInput.ChosenOptionId;
                existingAnswer.AnswerText = answerInput.AnswerText;
            }
            else
            {
                var newAnswer = new UserAnswer
                {
                    SessionId = answerInput.SessionId,
                    QuestionId = answerInput.QuestionId,
                    ChosenOptionId = answerInput.ChosenOptionId,
                    AnswerText = answerInput.AnswerText,
                };
                _context.UserAnswers.Add(newAnswer);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTest(TakeTestViewModel viewModel)
        {
            return await ProcessAndSubmitTest(viewModel.SessionId, viewModel.UserAnswers);
        }

        // === PHẦN SỬA LỖI NẰM Ở ĐÂY ===
        private async Task<IActionResult> ProcessAndSubmitTest(int sessionId, List<UserAnswerInput> submittedAnswers)
        {
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId)) { return Unauthorized(); }

            var session = await _context.UserTestSessions
                .Include(s => s.Test)
                    .ThenInclude(t => t.Questions)
                        .ThenInclude(q => q.AnswerOptions)
                .Include(s => s.UserAnswers)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

            if (session == null || session.Status != "IN_PROGRESS")
            {
                TempData["ErrorMessage"] = "Không thể nộp bài cho phiên làm bài này.";
                return RedirectToAction("MyTests", "EmployeeDashboard");
            }

            // BƯỚC 1: LƯU LẠI TOÀN BỘ CÂU TRẢ LỜI
            foreach (var submittedAnswer in submittedAnswers)
            {
                if (!submittedAnswer.ChosenOptionId.HasValue && string.IsNullOrWhiteSpace(submittedAnswer.AnswerText)) continue;

                var existingAnswer = session.UserAnswers.FirstOrDefault(ua => ua.QuestionId == submittedAnswer.QuestionId);
                if (existingAnswer != null)
                {
                    existingAnswer.ChosenOptionId = submittedAnswer.ChosenOptionId;
                    existingAnswer.AnswerText = submittedAnswer.AnswerText;
                }
                else
                {
                    var newAnswer = new UserAnswer
                    {
                        SessionId = sessionId,
                        QuestionId = submittedAnswer.QuestionId,
                        ChosenOptionId = submittedAnswer.ChosenOptionId,
                        AnswerText = submittedAnswer.AnswerText,
                    };
                    _context.UserAnswers.Add(newAnswer);
                    session.UserAnswers.Add(newAnswer);
                }
            }
            await _context.SaveChangesAsync();

            // BƯỚC 2: CHẤM ĐIỂM TỰ ĐỘNG VÀ TÍNH TỔNG ĐIỂM TẠM THỜI
            int totalQuestions = session.Test.Questions.Count;
            decimal partialScore = 0;
            bool hasEssayQuestions = false;
            decimal pointsPerQuestion = (totalQuestions > 0) ? 10.0m / totalQuestions : 0;

            foreach (var question in session.Test.Questions)
            {
                var userAnswer = session.UserAnswers.FirstOrDefault(ua => ua.QuestionId == question.QuestionId);
                if (userAnswer == null) continue;

                if (question.QuestionType == "ESSAY")
                {
                    hasEssayQuestions = true;
                    userAnswer.Score = null; // Tự luận cần chờ chấm
                }
                else // Chấm tự động các câu trắc nghiệm
                {
                    var correctOption = question.AnswerOptions.FirstOrDefault(o => o.IsCorrect);
                    if (correctOption != null && userAnswer.ChosenOptionId == correctOption.OptionId)
                    {
                        userAnswer.Score = pointsPerQuestion;
                    }
                    else
                    {
                        userAnswer.Score = 0;
                    }
                    // Cộng điểm vừa chấm vào điểm tạm thời
                    partialScore += userAnswer.Score.Value;
                }
            }

            // BƯỚC 3: CẬP NHẬT PHIÊN LÀM BÀI
            session.EndTime = DateTime.Now;

            // SỬA LỖI: Luôn lưu điểm tạm thời của các câu trắc nghiệm
            session.FinalScore = Math.Round(partialScore, 2);

            // Trạng thái và kết quả cuối cùng (IsPassed) phụ thuộc vào việc có câu tự luận không
            if (hasEssayQuestions)
            {
                session.Status = "COMPLETED"; // Trạng thái: Đã nộp, chờ chấm
                session.IsPassed = null;      // Kết quả cuối cùng: Chưa xác định
            }
            else
            {
                session.Status = "GRADED";    // Trạng thái: Đã chấm xong (vì không có tự luận)
                session.IsPassed = session.FinalScore >= (session.Test?.PassingScore ?? 0);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bạn đã nộp bài thành công!";
            return RedirectToAction("ViewResult", "EmployeeDashboard", new { sessionId = session.SessionId });
        }
    }
}