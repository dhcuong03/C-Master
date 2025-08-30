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
                UserAnswers = session.Test.Questions.Select(q => new UserAnswerInput { QuestionId = q.QuestionId }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTest(TakeTestViewModel viewModel)
        {
            return await ProcessAndSubmitTest(viewModel.SessionId, viewModel.UserAnswers);
        }

        private async Task<IActionResult> ProcessAndSubmitTest(int sessionId, List<UserAnswerInput> userAnswers)
        {
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId)) { return Unauthorized(); }

            var session = await _context.UserTestSessions
                .Include(s => s.Test)
                    .ThenInclude(t => t.Questions)
                        .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

            if (session == null || session.Status != "IN_PROGRESS")
            {
                TempData["ErrorMessage"] = "Không thể nộp bài cho phiên làm bài này.";
                return RedirectToAction("MyTests", "EmployeeDashboard");
            }

            var oldAnswers = await _context.UserAnswers.Where(ua => ua.SessionId == sessionId).ToListAsync();
            if (oldAnswers.Any())
            {
                _context.UserAnswers.RemoveRange(oldAnswers);
            }

            int totalQuestions = session.Test.Questions.Count;
            decimal pointsPerQuestion = (totalQuestions > 0) ? 10.0m / totalQuestions : 0;
            int correctAnswersCount = 0;
            bool hasEssayQuestions = false;

            foreach (var question in session.Test.Questions)
            {
                var userAnswerInput = userAnswers?.FirstOrDefault(ua => ua.QuestionId == question.QuestionId);

                var userAnswer = new UserAnswer
                {
                    SessionId = session.SessionId,
                    QuestionId = question.QuestionId,
                    Score = 0
                };

                if (userAnswerInput != null)
                {
                    if (question.QuestionType == "MCQ" || question.QuestionType == "TRUE_FALSE")
                    {
                        userAnswer.ChosenOptionId = userAnswerInput.ChosenOptionId;
                        var correctOption = question.AnswerOptions.FirstOrDefault(o => o.IsCorrect == true);

                        if (correctOption != null && userAnswerInput.ChosenOptionId == correctOption.OptionId)
                        {
                            userAnswer.Score = pointsPerQuestion;
                            correctAnswersCount++;
                        }
                    }
                    else if (question.QuestionType == "ESSAY")
                    {
                        userAnswer.AnswerText = userAnswerInput.AnswerText;
                        userAnswer.Score = null;
                        hasEssayQuestions = true; // Đánh dấu là có câu tự luận
                    }
                }
                _context.UserAnswers.Add(userAnswer);
            }

            decimal finalScore = (totalQuestions > 0)
                ? ((decimal)correctAnswersCount / totalQuestions) * 10
                : 0;

            // Cập nhật phiên làm bài
            session.FinalScore = Math.Round(finalScore, 2);
            session.EndTime = DateTime.Now;
            session.Status = "COMPLETED";

            // === SỬA LỖI LOGIC: Tự động tính Đạt/Trượt nếu không có câu tự luận ===
            if (!hasEssayQuestions)
            {
                // Nếu không có câu tự luận, có thể xác định kết quả ngay
                session.IsPassed = session.FinalScore >= session.Test.PassingScore;
            }
            else
            {
                // Nếu có câu tự luận, để null và chờ người chấm
                session.IsPassed = null;
            }
            // =====================================================================

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bạn đã nộp bài thành công!";
            return RedirectToAction("MyTests", "EmployeeDashboard");
        }
    }
}
