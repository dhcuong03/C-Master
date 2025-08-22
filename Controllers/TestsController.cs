using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TestMaster.Models;

namespace TestMaster.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class TestsController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public TestsController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        // GET: Tests
        public async Task<IActionResult> Index()
        {
            var tests = await _context.Tests
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.Questions)
                .ToListAsync();
            return View(tests);
        }

        // GET: Tests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var test = await _context.Tests
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(m => m.TestId == id);

            if (test == null) return NotFound();

            return View(test);
        }

        // GET: Tests/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateTestViewModel();
            viewModel.AllQuestions = await _context.Questions
                .Include(q => q.Skill)
                .Select(q => new SelectableQuestion
                {
                    QuestionId = q.QuestionId,
                    Content = q.Content,
                    SkillName = q.Skill != null ? q.Skill.SkillName : "N/A",
                    Difficulty = q.Difficulty,
                    IsSelected = false
                }).ToListAsync();

            return View(viewModel);
        }

        // POST: Tests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTestViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var newTest = viewModel.Test;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (currentUserId != null)
                {
                    newTest.CreatedBy = int.Parse(currentUserId);
                }

                foreach (var selectableQuestion in viewModel.AllQuestions.Where(q => q.IsSelected))
                {
                    var questionToAdd = await _context.Questions.FindAsync(selectableQuestion.QuestionId);
                    if (questionToAdd != null)
                    {
                        newTest.Questions.Add(questionToAdd);
                    }
                }

                _context.Add(newTest);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo bài test thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Tests/Edit/5 (ĐÃ SỬA LỖI)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var test = await _context.Tests
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.TestId == id);

            if (test == null) return NotFound();

            // Lấy danh sách ID các câu hỏi đã có trong bài test ra một list riêng
            var selectedQuestionIds = test.Questions.Select(q => q.QuestionId).ToList();

            var viewModel = new CreateTestViewModel
            {
                Test = test,
                AllQuestions = await _context.Questions
                    .Include(q => q.Skill)
                    .Select(q => new SelectableQuestion
                    {
                        QuestionId = q.QuestionId,
                        Content = q.Content,
                        SkillName = q.Skill != null ? q.Skill.SkillName : "N/A",
                        Difficulty = q.Difficulty,
                        // Dùng list đã tạo ở trên để kiểm tra
                        IsSelected = selectedQuestionIds.Contains(q.QuestionId)
                    }).ToListAsync()
            };

            return View(viewModel);
        }


        // POST: Tests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateTestViewModel viewModel)
        {
            if (id != viewModel.Test.TestId) return NotFound();

            if (ModelState.IsValid)
            {
                var testToUpdate = await _context.Tests
                    .Include(t => t.Questions)
                    .FirstOrDefaultAsync(t => t.TestId == id);

                if (testToUpdate == null) return NotFound();

                testToUpdate.Title = viewModel.Test.Title;
                testToUpdate.Description = viewModel.Test.Description;
                testToUpdate.DurationMinutes = viewModel.Test.DurationMinutes;
                testToUpdate.PassingScore = viewModel.Test.PassingScore;

                testToUpdate.Questions.Clear();
                foreach (var selectableQuestion in viewModel.AllQuestions.Where(q => q.IsSelected))
                {
                    var questionToAdd = await _context.Questions.FindAsync(selectableQuestion.QuestionId);
                    if (questionToAdd != null)
                    {
                        testToUpdate.Questions.Add(questionToAdd);
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật bài test thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestExists(testToUpdate.TestId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Tests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var test = await _context.Tests
                .Include(t => t.CreatedByNavigation)
                .FirstOrDefaultAsync(m => m.TestId == id);

            if (test == null) return NotFound();

            return View(test);
        }

        // POST: Tests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var test = await _context.Tests.FindAsync(id);
            if (test != null)
            {
                _context.Tests.Remove(test);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa bài test thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TestExists(int id)
        {
            return _context.Tests.Any(e => e.TestId == id);
        }
    }
}
