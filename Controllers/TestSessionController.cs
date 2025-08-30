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

        // Action StartTest không thay đổi
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

        // === CẬP NHẬT: Tải lại các câu trả lời đã lưu ===
        [HttpGet]
        public async Task<IActionResult> TakeTest(int sessionId)
        {
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId)) { return Unauthorized(); }

            var session = await _context.UserTestSessions
                .Include(s => s.Test)
                    .ThenInclude(t => t.Questions)
                        .ThenInclude(q => q.AnswerOptions)
                // Tải kèm các câu trả lời đã được lưu
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
                return await ProcessAndSubmitTest(session.SessionId);
            }

            // Tạo ViewModel và điền các câu trả lời đã lưu vào
            var viewModel = new TakeTestViewModel
            {
                SessionId = session.SessionId,
                Test = session.Test,
                TimeRemainingInSeconds = timeRemaining,
                UserAnswers = session.Test.Questions.Select(q => {
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

        // === HÀM MỚI: Action để tự động lưu từng câu trả lời ===
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

        // === CẬP NHẬT: Đơn giản hóa Action này ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTest(TakeTestViewModel viewModel)
        {
            return await ProcessAndSubmitTest(viewModel.SessionId);
        }

        // === CẬP NHẬT: Phương thức này giờ sẽ đọc câu trả lời từ DB ===
        private async Task<IActionResult> ProcessAndSubmitTest(int sessionId)
        {
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId)) { return Unauthorized(); }

            var session = await _context.UserTestSessions
                .Include(s => s.Test)
                    .ThenInclude(t => t.Questions)
                        .ThenInclude(q => q.AnswerOptions)
                .Include(s => s.UserAnswers) // Tải các câu trả lời đã được auto-save
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

            if (session == null || session.Status != "IN_PROGRESS")
            {
                TempData["ErrorMessage"] = "Không thể nộp bài cho phiên làm bài này.";
                return RedirectToAction("MyTests", "EmployeeDashboard");
            }

            // Logic tính điểm dựa trên các câu trả lời đã được lưu
            int totalQuestions = session.Test.Questions.Count;
            int correctAnswersCount = 0;
            bool hasEssayQuestions = false;

            foreach (var question in session.Test.Questions)
            {
                var userAnswer = session.UserAnswers.FirstOrDefault(ua => ua.QuestionId == question.QuestionId);
                if (userAnswer == null) continue;

                if (question.QuestionType == "MCQ" || question.QuestionType == "TRUE_FALSE")
                {
                    var correctOption = question.AnswerOptions.FirstOrDefault(o => o.IsCorrect == true);
                    if (correctOption != null && userAnswer.ChosenOptionId == correctOption.OptionId)
                    {
                        correctAnswersCount++;
                    }
                }
                else if (question.QuestionType == "ESSAY" && !string.IsNullOrWhiteSpace(userAnswer.AnswerText))
                {
                    hasEssayQuestions = true;
                }
            }

            decimal finalScore = (totalQuestions > 0)
                ? ((decimal)correctAnswersCount / totalQuestions) * 10
                : 0;

            session.FinalScore = Math.Round(finalScore, 2);
            session.EndTime = DateTime.Now;
            session.Status = "COMPLETED";

            if (!hasEssayQuestions)
            {
                session.IsPassed = session.FinalScore >= session.Test.PassingScore;
            }
            else
            {
                session.IsPassed = null;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bạn đã nộp bài thành công!";
            return RedirectToAction("MyTests", "EmployeeDashboard");
        }
    }
}

