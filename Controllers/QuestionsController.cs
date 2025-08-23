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

        // POST: Questions/ImportFromExcel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một file Excel.";
                return RedirectToAction(nameof(Index));
            }

            var questionsToImport = new List<Question>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var currentUserId = User.FindFirst("UserId")?.Value;
            int? createdById = null;
            if (currentUserId != null)
            {
                createdById = int.Parse(currentUserId);
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        TempData["ErrorMessage"] = "File Excel không hợp lệ.";
                        return RedirectToAction(nameof(Index));
                    }

                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            string content = worksheet.Cells[row, 1].Value?.ToString().Trim();
                            if (string.IsNullOrEmpty(content)) continue;

                            var question = new Question
                            {
                                Content = content,
                                QuestionType = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                                Difficulty = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now,
                                CreatedBy = createdById,
                                AnswerOptions = new List<AnswerOption>()
                            };

                            string skillIdString = worksheet.Cells[row, 4].Value?.ToString().Trim();
                            if (int.TryParse(skillIdString, out int parsedSkillId))
                            {
                                question.SkillId = parsedSkillId;
                            }

                            for (int col = 5; col <= worksheet.Dimension.Columns; col += 2)
                            {
                                var optionText = worksheet.Cells[row, col].Value?.ToString().Trim();
                                var isCorrectString = worksheet.Cells[row, col + 1].Value?.ToString().Trim();

                                if (string.IsNullOrEmpty(optionText)) break;

                                bool.TryParse(isCorrectString, out bool isCorrect);

                                question.AnswerOptions.Add(new AnswerOption
                                {
                                    OptionText = optionText,
                                    IsCorrect = isCorrect
                                });
                            }

                            questionsToImport.Add(question);
                        }
                        catch (Exception)
                        {
                            TempData["ErrorMessage"] = $"Định dạng dữ liệu ở dòng {row} không hợp lệ. Vui lòng kiểm tra lại.";
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }

            if (questionsToImport.Any())
            {
                await _context.Questions.AddRangeAsync(questionsToImport);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã import thành công {questionsToImport.Count} câu hỏi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy dữ liệu hợp lệ trong file Excel.";
            }

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

            var existingOptions = _context.AnswerOptions.Where(ao => ao.QuestionId == id);
            _context.AnswerOptions.RemoveRange(existingOptions);

            question.AnswerOptions = question.AnswerOptions.Where(opt => !string.IsNullOrWhiteSpace(opt.OptionText)).ToList();
            foreach (var option in question.AnswerOptions)
            {
                option.QuestionId = id;
                _context.AnswerOptions.Add(option);
            }

            try
            {
                _context.Update(question);
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