using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TestMaster.Models;

namespace TestMaster.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class QuestionsController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public QuestionsController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        // GET: Questions
        public async Task<IActionResult> Index()
        {
            var questions = _context.Questions.Include(q => q.Skill);
            return View(await questions.ToListAsync());
        }

        // GET: Questions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var question = await _context.Questions
                .Include(q => q.Skill)
                .Include(q => q.AnswerOptions) // Lấy cả các đáp án
                .FirstOrDefaultAsync(m => m.QuestionId == id);
            if (question == null) return NotFound();
            return View(question);
        }

        // GET: Questions/Create
        public IActionResult Create()
        {
            ViewData["SkillId"] = new SelectList(_context.Skills, "SkillId", "SkillName");
            ViewData["QuestionTypeList"] = new SelectList(new[] { "MCQ", "ESSAY", "TRUE_FALSE" });
            ViewData["DifficultyList"] = new SelectList(new[] { "JUNIOR", "MIDDLE", "SENIOR" });

            var question = new Question
            {
                AnswerOptions = new List<AnswerOption> { new(), new(), new(), new() }
            };
            return View(question);
        }

        // POST: Questions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question question)
        {
            question.AnswerOptions = question.AnswerOptions.Where(opt => !string.IsNullOrWhiteSpace(opt.OptionText)).ToList();

            var currentUserId = User.FindFirst("UserId")?.Value;
            if (currentUserId != null)
            {
                question.CreatedBy = int.Parse(currentUserId);
            }

            _context.Add(question);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tạo câu hỏi thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Questions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var question = await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null) return NotFound();

            ViewData["SkillId"] = new SelectList(_context.Skills, "SkillId", "SkillName", question.SkillId);
            ViewData["QuestionTypeList"] = new SelectList(new[] { "MCQ", "ESSAY", "TRUE_FALSE" }, question.QuestionType);
            ViewData["DifficultyList"] = new SelectList(new[] { "JUNIOR", "MIDDLE", "SENIOR" }, question.Difficulty);
            return View(question);
        }

        // POST: Questions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Question question)
        {
            if (id != question.QuestionId) return NotFound();

            // Lấy các đáp án cũ từ CSDL để xóa
            var existingOptions = _context.AnswerOptions.AsNoTracking().Where(ao => ao.QuestionId == id);
            _context.AnswerOptions.RemoveRange(existingOptions);

            // Lọc và thêm các đáp án mới từ form
            question.AnswerOptions = question.AnswerOptions.Where(opt => !string.IsNullOrWhiteSpace(opt.OptionText)).ToList();
            foreach (var option in question.AnswerOptions)
            {
                option.QuestionId = id; // Đảm bảo khóa ngoại đúng
                _context.AnswerOptions.Add(option);
            }

            try
            {
                _context.Update(question);
                // Báo cho EF không theo dõi lại các đáp án (vì ta đã xử lý riêng)
                _context.Entry(question).Collection(q => q.AnswerOptions).IsModified = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật câu hỏi thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionExists(question.QuestionId)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Questions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var question = await _context.Questions
                .Include(q => q.Skill)
                .FirstOrDefaultAsync(m => m.QuestionId == id);
            if (question == null) return NotFound();
            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa câu hỏi thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.QuestionId == id);
        }
    }
}
