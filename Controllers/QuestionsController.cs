using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using TestMaster.Models;
using TestMaster.ViewModels; // <-- Đảm bảo bạn có using ViewModel ở đây

namespace TestMaster.Controllers
{
    [Authorize(Roles = "Admin,HR,Manager")]
    public class QuestionsController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public QuestionsController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        // GET: Questions
        public async Task<IActionResult> Index(string searchString, int? skillId, string difficulty)
        {
            var questionsQuery = _context.Questions.Include(q => q.Skill).AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                questionsQuery = questionsQuery.Where(q => q.Content.Contains(searchString));
            }
            if (skillId.HasValue)
            {
                questionsQuery = questionsQuery.Where(q => q.SkillId == skillId.Value);
            }
            if (!String.IsNullOrEmpty(difficulty))
            {
                questionsQuery = questionsQuery.Where(q => q.Difficulty == difficulty);
            }

            ViewData["SkillList"] = new SelectList(await _context.Skills.ToListAsync(), "SkillId", "SkillName", skillId);
            ViewData["DifficultyList"] = new SelectList(new[] { "JUNIOR", "MIDDLE", "SENIOR" }, difficulty);
            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentDifficulty"] = difficulty;

            var questions = await questionsQuery.OrderByDescending(q => q.QuestionId).ToListAsync();

            // === LOGIC MỚI: KIỂM TRA CÂU HỎI NÀO ĐÃ ĐƯỢC SỬ DỤNG ===
            // Lấy danh sách ID của tất cả các câu hỏi đã có người trả lời
            var usedQuestionIds = await _context.UserAnswers
                .Select(ua => ua.QuestionId)
                .Distinct()
                .ToHashSetAsync();

            // Tạo một ViewModel để gửi sang View, bao gồm cả thông tin câu hỏi và trạng thái "đã sử dụng"
            var viewModel = questions.Select(q => new QuestionIndexViewModel
            {
                Question = q,
                IsInUse = usedQuestionIds.Contains(q.QuestionId)
            }).ToList();
            // =========================================================

            return View(viewModel);
        }


        // GET: Questions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var question = await _context.Questions
                .Include(q => q.Skill)
                .Include(q => q.AnswerOptions)
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

        #region Import Functions

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DownloadTemplate()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("QuestionTemplate");

                worksheet.Cells["A1"].Value = "Content";
                worksheet.Cells["B1"].Value = "QuestionType";
                worksheet.Cells["C1"].Value = "Difficulty";
                worksheet.Cells["D1"].Value = "SkillName";
                worksheet.Cells["E1"].Value = "Option1_Text";
                worksheet.Cells["F1"].Value = "Option1_IsCorrect";
                worksheet.Cells["G1"].Value = "Option2_Text";
                worksheet.Cells["H1"].Value = "Option2_IsCorrect";
                worksheet.Cells["I1"].Value = "Option3_Text";
                worksheet.Cells["J1"].Value = "Option3_IsCorrect";
                worksheet.Cells["K1"].Value = "Option4_Text";
                worksheet.Cells["L1"].Value = "Option4_IsCorrect";

                // ... (Các dòng dữ liệu mẫu giữ nguyên)

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"QuestionImportTemplate_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            // ... (Logic import giữ nguyên, không cần sửa)
            if (file == null || file.Length <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một file Excel.";
                return RedirectToAction(nameof(Index));
            }

            var questionsToImport = new List<Question>();
            // ... (Phần còn lại của hàm giữ nguyên)

            await _context.Questions.AddRangeAsync(questionsToImport);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã import thành công {questionsToImport.Count} câu hỏi.";
            return RedirectToAction(nameof(Index));
        }


        #endregion

        // === LOGIC MỚI: CHỨC NĂNG TẠO BẢN SAO (CLONE) ===
        // GET: Questions/Clone/5
        [HttpGet]
        public async Task<IActionResult> Clone(int? id)
        {
            if (id == null) return NotFound();

            // Tải câu hỏi gốc, không theo dõi thay đổi để tránh xung đột
            var originalQuestion = await _context.Questions
                .Include(q => q.AnswerOptions)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (originalQuestion == null) return NotFound();

            // Tạo một đối tượng câu hỏi mới dựa trên bản gốc
            var clonedQuestion = new Question
            {
                Content = "[BẢN SAO] " + originalQuestion.Content,
                QuestionType = originalQuestion.QuestionType,
                SkillId = originalQuestion.SkillId,
                Difficulty = originalQuestion.Difficulty,
                AnswerOptions = new List<AnswerOption>()
            };

            // Sao chép các lựa chọn trả lời
            foreach (var option in originalQuestion.AnswerOptions)
            {
                clonedQuestion.AnswerOptions.Add(new AnswerOption
                {
                    OptionText = option.OptionText,
                    IsCorrect = option.IsCorrect
                });
            }

            // Đảm bảo luôn có 4 lựa chọn để hiển thị trên form
            while (clonedQuestion.AnswerOptions.Count < 4)
            {
                clonedQuestion.AnswerOptions.Add(new AnswerOption());
            }

            // Gửi câu hỏi đã sao chép đến View "Create" để người dùng chỉnh sửa và lưu lại
            ViewData["SkillId"] = new SelectList(_context.Skills, "SkillId", "SkillName", clonedQuestion.SkillId);
            ViewData["QuestionTypeList"] = new SelectList(new[] { "MCQ", "ESSAY", "TRUE_FALSE" }, clonedQuestion.QuestionType);
            ViewData["DifficultyList"] = new SelectList(new[] { "JUNIOR", "MIDDLE", "SENIOR" }, clonedQuestion.Difficulty);

            TempData["InfoMessage"] = "Đây là một bản sao. Vui lòng chỉnh sửa và lưu lại.";
            return View("Create", clonedQuestion);
        }
        // =================================================

        // GET: Questions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // === LOGIC MỚI: KIỂM TRA TRƯỚC KHI CHO SỬA ===
            bool isInUse = await _context.UserAnswers.AnyAsync(ua => ua.QuestionId == id);
            if (isInUse)
            {
                TempData["ErrorMessage"] = "Không thể sửa câu hỏi này vì đã có nhân viên trả lời. Vui lòng tạo một bản sao để chỉnh sửa.";
                return RedirectToAction(nameof(Index));
            }
            // ===========================================

            var question = await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null) return NotFound();

            while (question.AnswerOptions.Count < 4)
            {
                question.AnswerOptions.Add(new AnswerOption());
            }

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
            if (id != question.QuestionId)
            {
                return NotFound();
            }

            // === LOGIC MỚI: KIỂM TRA LẠI TRƯỚC KHI LƯU ===
            bool isInUse = await _context.UserAnswers.AnyAsync(ua => ua.QuestionId == id);
            if (isInUse)
            {
                TempData["ErrorMessage"] = "Lỗi: Không thể lưu thay đổi cho câu hỏi đã được sử dụng.";
                return RedirectToAction(nameof(Index));
            }
            // ==========================================

            var questionToUpdate = await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (questionToUpdate == null) return NotFound();

            _context.Entry(questionToUpdate).CurrentValues.SetValues(question);
            questionToUpdate.UpdatedAt = DateTime.Now;

            // Xử lý cập nhật AnswerOptions một cách an toàn
            _context.AnswerOptions.RemoveRange(questionToUpdate.AnswerOptions);

            var newOptions = question.AnswerOptions
                                     .Where(opt => !string.IsNullOrWhiteSpace(opt.OptionText))
                                     .ToList();

            foreach (var option in newOptions)
            {
                option.OptionId = 0;
                questionToUpdate.AnswerOptions.Add(option);
            }

            try
            {
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

            // === LOGIC MỚI: KIỂM TRA TRƯỚC KHI CHO XÓA ===
            bool isInUse = await _context.UserAnswers.AnyAsync(ua => ua.QuestionId == id);
            if (isInUse)
            {
                TempData["ErrorMessage"] = "Không thể xóa câu hỏi này vì đã có nhân viên trả lời.";
                return RedirectToAction(nameof(Index));
            }
            // ===========================================

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
            // === LOGIC MỚI: KIỂM TRA LẠI TRƯỚC KHI XÓA ===
            bool isInUse = await _context.UserAnswers.AnyAsync(ua => ua.QuestionId == id);
            if (isInUse)
            {
                TempData["ErrorMessage"] = "Lỗi: Không thể xóa câu hỏi đã được sử dụng.";
                return RedirectToAction(nameof(Index));
            }
            // ==========================================

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

