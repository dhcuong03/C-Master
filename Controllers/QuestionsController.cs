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
            return View(questions);
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

                worksheet.Cells["A2"].Value = "Câu lệnh SQL nào được dùng để truy vấn dữ liệu?";
                worksheet.Cells["B2"].Value = "MCQ";
                worksheet.Cells["C2"].Value = "JUNIOR";
                worksheet.Cells["D2"].Value = "Business Analyst";
                worksheet.Cells["E2"].Value = "GET";
                worksheet.Cells["F2"].Value = "FALSE";
                worksheet.Cells["G2"].Value = "SELECT";
                worksheet.Cells["H2"].Value = "TRUE";
                worksheet.Cells["I2"].Value = "QUERY";
                worksheet.Cells["J2"].Value = "FALSE";
                worksheet.Cells["K2"].Value = "FETCH";
                worksheet.Cells["L2"].Value = "FALSE";

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

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
            if (file == null || file.Length <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một file Excel.";
                return RedirectToAction(nameof(Index));
            }

            var questionsToImport = new List<Question>();
            var skillsFromDb = await _context.Skills.ToDictionaryAsync(s => s.SkillName.ToLower(), s => s.SkillId);
            var validQuestionTypes = new List<string> { "MCQ", "ESSAY", "TRUE_FALSE" };
            var validDifficulties = new List<string> { "JUNIOR", "MIDDLE", "SENIOR" };

            var currentUserId = User.FindFirst("UserId")?.Value;
            int? createdById = currentUserId != null ? int.Parse(currentUserId) : null;

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
                            string content = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                            if (string.IsNullOrEmpty(content)) continue;

                            string questionTypeRaw = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                            if (string.IsNullOrEmpty(questionTypeRaw) || !validQuestionTypes.Contains(questionTypeRaw.ToUpper()))
                            {
                                TempData["ErrorMessage"] = $"Lỗi ở dòng {row}: Loại câu hỏi '{questionTypeRaw}' không hợp lệ hoặc bị bỏ trống.";
                                return RedirectToAction(nameof(Index));
                            }
                            string questionType = questionTypeRaw.ToUpper();

                            string difficultyRaw = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                            if (string.IsNullOrEmpty(difficultyRaw) || !validDifficulties.Contains(difficultyRaw.ToUpper()))
                            {
                                TempData["ErrorMessage"] = $"Lỗi ở dòng {row}: Độ khó '{difficultyRaw}' không hợp lệ hoặc bị bỏ trống.";
                                return RedirectToAction(nameof(Index));
                            }
                            string difficulty = difficultyRaw.ToUpper();

                            string skillName = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                            if (string.IsNullOrEmpty(skillName) || !skillsFromDb.TryGetValue(skillName.ToLower(), out int skillId))
                            {
                                TempData["ErrorMessage"] = $"Lỗi ở dòng {row}: Kỹ năng '{skillName}' không tồn tại trong hệ thống hoặc bị bỏ trống.";
                                return RedirectToAction(nameof(Index));
                            }

                            var question = new Question
                            {
                                Content = content,
                                QuestionType = questionType,
                                Difficulty = difficulty,
                                SkillId = skillId,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now,
                                CreatedBy = createdById,
                                AnswerOptions = new List<AnswerOption>()
                            };

                            for (int col = 5; col <= 11; col += 2)
                            {
                                var optionText = worksheet.Cells[row, col].Value?.ToString()?.Trim();
                                if (string.IsNullOrEmpty(optionText)) continue;

                                var isCorrectString = worksheet.Cells[row, col + 1].Value?.ToString()?.Trim();
                                bool.TryParse(isCorrectString, out bool isCorrect);

                                question.AnswerOptions.Add(new AnswerOption
                                {
                                    OptionText = optionText,
                                    IsCorrect = isCorrect
                                });
                            }
                            questionsToImport.Add(question);
                        }
                        catch (Exception ex)
                        {
                            TempData["ErrorMessage"] = $"Định dạng dữ liệu ở dòng {row} không hợp lệ. Chi tiết: {ex.Message}";
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

        #endregion

        // GET: Questions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

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
        // === TOÀN BỘ PHƯƠNG THỨC NÀY ĐÃ ĐƯỢC CẬP NHẬT ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Question question)
        {
            if (id != question.QuestionId)
            {
                return NotFound();
            }

            // Tải đối tượng Question gốc từ DB, bao gồm cả các AnswerOption hiện có
            var questionToUpdate = await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (questionToUpdate == null)
            {
                return NotFound();
            }

            // Cập nhật các thuộc tính của Question (Content, Difficulty, SkillId...)
            _context.Entry(questionToUpdate).CurrentValues.SetValues(question);
            questionToUpdate.UpdatedAt = DateTime.Now;

            // [SỬA LỖI & TỐI ƯU]
            // Thay vì xóa rồi lưu, sau đó thêm rồi lại lưu (2 lần),
            // chúng ta sẽ thực hiện tất cả thay đổi trong bộ nhớ trước,
            // sau đó gọi SaveChangesAsync() một lần duy nhất ở cuối.
            // Điều này đảm bảo toàn bộ thao tác là một transaction, an toàn hơn.

            // 1. Đánh dấu các AnswerOption cũ để xóa khỏi context
            _context.AnswerOptions.RemoveRange(questionToUpdate.AnswerOptions);

            // 2. Xử lý các AnswerOption mới từ form gửi lên
            var newOptions = question.AnswerOptions
                                     .Where(opt => !string.IsNullOrWhiteSpace(opt.OptionText))
                                     .ToList();

            foreach (var option in newOptions)
            {
                // [FIX] Đây là dòng sửa lỗi chính:
                // Reset ID về 0 để Entity Framework hiểu đây là bản ghi mới cần INSERT,
                // và để database tự sinh ID mới cho nó.
                option.OptionId = 0;

                // Thêm option mới vào collection của question đang được theo dõi.
                // EF sẽ tự động biết đây là bản ghi cần 'Add'.
                questionToUpdate.AnswerOptions.Add(option);
            }

            try
            {
                // 3. Gọi SaveChangesAsync một lần duy nhất để áp dụng tất cả thay đổi (xóa và thêm) vào DB
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật câu hỏi thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionExists(question.QuestionId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
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