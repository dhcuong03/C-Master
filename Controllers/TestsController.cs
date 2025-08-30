using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                .Include(t => t.Level)
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
                .Include(t => t.Level)
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

            ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName");

            return View(viewModel);
        }

        // POST: Tests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTestViewModel viewModel)
        {
            ModelState.Remove("AllQuestions");

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

            ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName", viewModel.Test.LevelId);
            return View(viewModel);
        }

        // GET: Tests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var test = await _context.Tests
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.TestId == id);

            if (test == null) return NotFound();

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
                        IsSelected = selectedQuestionIds.Contains(q.QuestionId)
                    }).ToListAsync()
            };

            ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName", test.LevelId);

            return View(viewModel);
        }

        // POST: Tests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateTestViewModel viewModel)
        {
            if (id != viewModel.Test.TestId) return NotFound();

            ModelState.Remove("AllQuestions");

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
                testToUpdate.LevelId = viewModel.Test.LevelId;

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
            ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName", viewModel.Test.LevelId);
            return View(viewModel);
        }

        // GET: Tests/Assign/5
        [HttpGet]
        public async Task<IActionResult> Assign(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests.FindAsync(id);
            if (test == null)
            {
                return NotFound();
            }

            var employeeUsers = _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleName == "Employee")
                .OrderBy(u => u.FullName);

            var viewModel = new AssignTestViewModel
            {
                TestId = test.TestId,
                TestTitle = test.Title,
                UsersList = new SelectList(await employeeUsers.ToListAsync(), "UserId", "FullName"),
                DepartmentsList = new SelectList(_context.Departments.OrderBy(d => d.DepartmentName), "DepartmentId", "DepartmentName"),
                LevelsList = new SelectList(_context.Levels.OrderBy(l => l.LevelName), "LevelId", "LevelName")
            };

            return View(viewModel);
        }

        // POST: Tests/Assign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignTestViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var employeeUsers = _context.Users.Include(u => u.Role).Where(u => u.Role.RoleName == "Employee").OrderBy(u => u.FullName);
                viewModel.UsersList = new SelectList(await employeeUsers.ToListAsync(), "UserId", "FullName", viewModel.SelectedUserId);
                viewModel.DepartmentsList = new SelectList(_context.Departments.OrderBy(d => d.DepartmentName), "DepartmentId", "DepartmentName", viewModel.SelectedDepartmentId);
                viewModel.LevelsList = new SelectList(_context.Levels.OrderBy(l => l.LevelName), "LevelId", "LevelName", viewModel.SelectedLevelId);
                return View(viewModel);
            }

            var currentUserId = int.Parse(User.FindFirstValue("UserId"));
            var newAssignments = new List<TestAssignment>();

            switch (viewModel.AssignTo)
            {
                case "User":
                    if (viewModel.SelectedUserId.HasValue)
                    {
                        newAssignments.Add(new TestAssignment
                        {
                            TestId = viewModel.TestId,
                            UserId = viewModel.SelectedUserId.Value,
                            DueDate = viewModel.DueDate,
                            AssignedBy = currentUserId,
                            AssignedAt = DateTime.Now
                        });
                    }
                    break;

                case "Department":
                    if (viewModel.SelectedDepartmentId.HasValue)
                    {
                        var usersInDept = await _context.Users
                            .Where(u => u.DepartmentId == viewModel.SelectedDepartmentId.Value)
                            .ToListAsync();

                        foreach (var user in usersInDept)
                        {
                            newAssignments.Add(new TestAssignment
                            {
                                TestId = viewModel.TestId,
                                UserId = user.UserId,
                                DueDate = viewModel.DueDate,
                                AssignedBy = currentUserId,
                                AssignedAt = DateTime.Now
                            });
                        }
                    }
                    break;

                case "Level":
                    if (viewModel.SelectedLevelId.HasValue)
                    {
                        var usersInLevel = await _context.Users
                            .Where(u => u.LevelId == viewModel.SelectedLevelId.Value)
                            .ToListAsync();

                        foreach (var user in usersInLevel)
                        {
                            newAssignments.Add(new TestAssignment
                            {
                                TestId = viewModel.TestId,
                                UserId = user.UserId,
                                DueDate = viewModel.DueDate,
                                AssignedBy = currentUserId,
                                AssignedAt = DateTime.Now
                            });
                        }
                    }
                    break;
            }

            if (newAssignments.Any())
            {
                await _context.TestAssignments.AddRangeAsync(newAssignments);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã giao bài test thành công cho {newAssignments.Count} nhân viên.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhân viên nào phù hợp để giao bài.";
            }

            return RedirectToAction(nameof(ManageAssignments));
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
            var test = await _context.Tests
                .Include(t => t.TestAssignments)
                .Include(t => t.UserTestSessions)
                .FirstOrDefaultAsync(t => t.TestId == id);

            if (test != null)
            {
                _context.UserTestSessions.RemoveRange(test.UserTestSessions);
                _context.TestAssignments.RemoveRange(test.TestAssignments);
                _context.Tests.Remove(test);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa bài test và tất cả dữ liệu liên quan thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // =======================================================================
        // ===== HÀM MANAGE ASSIGNMENTS ĐÃ ĐƯỢC SỬA LẠI VÀ TỐI ƯU =====
        // =======================================================================
        [HttpGet]
        public async Task<IActionResult> ManageAssignments()
        {
            // 1. Lấy tất cả lượt giao bài. Giờ đây mỗi lượt đều đã có UserId.
            var allAssignments = await _context.TestAssignments
                .Include(a => a.Test)
                .Include(a => a.User) // Quan trọng: phải có thông tin User đi kèm
                .Where(a => a.User != null && a.Test != null) // Lọc bỏ các bản ghi cũ hoặc không hợp lệ
                .OrderByDescending(a => a.AssignedAt)
                .ToListAsync();

            // 2. Lấy trạng thái làm bài để hiển thị ("Chưa làm", "Đã nộp"...)
            var allSessions = await _context.UserTestSessions
                .ToDictionaryAsync(s => $"{s.UserId}-{s.TestId}", s => s.Status);

            // 3. Tạo ViewModel từ danh sách lượt giao bài đã lấy được
            var viewModel = allAssignments.Select(a => new AssignmentViewModel
            {
                Assignment = a,
                Status = GetStatusString(allSessions.TryGetValue($"{a.UserId}-{a.TestId}", out var status) ? status : null)
            }).ToList();

            // 4. Trả về View "Assignments.cshtml" với danh sách ViewModel đã được tạo
            return View("Assignments", viewModel);
        }

        private string GetStatusString(string dbStatus)
        {
            return dbStatus switch
            {
                "IN_PROGRESS" => "Đang làm",
                "COMPLETED" => "Đã nộp",
                "GRADED" => "Đã chấm",
                _ => "Chưa làm"
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAssignment(int assignmentId)
        {
            var assignment = await _context.TestAssignments.FindAsync(assignmentId);
            if (assignment == null)
            {
                return NotFound();
            }

            _context.TestAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã xóa lượt giao bài thành công!";

            return RedirectToAction(nameof(ManageAssignments));
        }

        private bool TestExists(int id)
        {
            return _context.Tests.Any(e => e.TestId == id);
        }
    }
}