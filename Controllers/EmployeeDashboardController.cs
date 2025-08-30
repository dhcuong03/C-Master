using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestMaster.Models;
using TestMaster.ViewModels;

namespace TestMaster.Controllers
{
    [Authorize(Roles = "Employee,Manager,Admin,HR")]
    public class EmployeeDashboardController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public EmployeeDashboardController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        #region Profile Actions
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId)) { return Unauthorized(); }
            var userProfile = await _context.Users
                .Include(u => u.Role).Include(u => u.Level).Include(u => u.Department)
                .AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
            if (userProfile == null) return NotFound();
            var testHistory = await _context.UserTestSessions
                .Where(s => s.UserId == userId).Include(s => s.Test)
                .OrderByDescending(s => s.StartTime).ToListAsync();
            var viewModel = new ProfileViewEmployee
            {
                UserProfile = userProfile,
                TestHistory = testHistory
            };
            return View(viewModel);
        }

        public async Task<IActionResult> EditProfile()
        {
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId)) { return Unauthorized(); }
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile([Bind("UserId,FullName,Email")] User userForm)
        {
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId) || userId != userForm.UserId) { return Unauthorized(); }
            var userToUpdate = await _context.Users.FindAsync(userId);
            if (userToUpdate == null) return NotFound();
            userToUpdate.FullName = userForm.FullName;
            userToUpdate.Email = userForm.Email;
            userToUpdate.UpdatedAt = System.DateTime.Now;
            ModelState.Remove("PasswordHash");
            ModelState.Remove("Username");
            if (ModelState.IsValid)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Cập nhật thất bại. Email có thể đã tồn tại.";
                }
            }
            return View(userForm);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) { return View(model); }
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId)) { return Unauthorized(); }
            var user = await _context.Users.FindAsync(userId);
            if (user == null) { return NotFound(); }
            if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash))
            {
                ModelState.AddModelError("OldPassword", "Mật khẩu cũ không chính xác.");
                return View(model);
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.UpdatedAt = System.DateTime.Now;
            _context.Update(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction(nameof(Index));
        }
        #endregion

        [HttpGet]
        public async Task<IActionResult> MyTests()
        {
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId)) { return Unauthorized(); }
            var user = await _context.Users.FindAsync(userId);
            if (user == null) { return NotFound(); }

            // 1. Lấy các bài test được GIAO THỦ CÔNG
            var manuallyAssignedTests = await _context.TestAssignments
                .Include(ta => ta.Test)
                .Where(ta => ta.Test != null && (ta.UserId == userId || (ta.DepartmentId == user.DepartmentId && ta.UserId == null)))
                .Select(ta => ta.Test)
                .ToListAsync();

            // 2. Lấy các bài test TỰ ĐỘNG THEO TRÌNH ĐỘ
            List<Test> levelBasedTests = new List<Test>();
            if (user.LevelId != null)
            {
                var specificallyAssignedTestIds = await _context.TestAssignments
                    .Where(a => a.UserId != null && a.TestId.HasValue)
                    .Select(a => a.TestId.Value)
                    .Distinct()
                    .ToListAsync();

                levelBasedTests = await _context.Tests
                    .Where(t => t.LevelId == user.LevelId && !specificallyAssignedTestIds.Contains(t.TestId))
                    .ToListAsync();
            }

            // 3. Gộp hai danh sách lại và loại bỏ các bài bị trùng lặp
            var allTests = manuallyAssignedTests.Union(levelBasedTests).Distinct().OrderBy(t => t.Title).ToList();

            // === SỬA LỖI: Xử lý trường hợp có nhiều phiên làm bài cho cùng một bài test ===
            // Lấy lịch sử làm bài, nhóm theo TestId và chỉ chọn phiên MỚI NHẤT cho mỗi bài test.
            var userSessions = (await _context.UserTestSessions
                .Where(s => s.UserId == userId && s.TestId.HasValue)
                .ToListAsync()) // Lấy dữ liệu về bộ nhớ trước khi nhóm
                .GroupBy(s => s.TestId)
                .ToDictionary(g => g.Key.Value, g => g.OrderByDescending(s => s.StartTime).First());

            ViewBag.UserSessions = userSessions;

            return View(allTests);
        }

        // ===== CHỨC NĂNG MỚI: XEM KẾT QUẢ BÀI TEST =====
        [HttpGet]
        public async Task<IActionResult> ViewResult(int sessionId)
        {
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId)) { return Unauthorized(); }

            var session = await _context.UserTestSessions
                .Include(s => s.Test)
                .Include(s => s.UserAnswers)
                    .ThenInclude(ua => ua.Question)
                        .ThenInclude(q => q.AnswerOptions)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

            if (session == null)
            {
                return NotFound();
            }

            // Sắp xếp lại câu trả lời theo đúng thứ tự câu hỏi trong bài test
            session.UserAnswers = session.UserAnswers.OrderBy(ua => ua.Question.QuestionId).ToList();

            var viewModel = new TestResultViewModel
            {
                Session = session
            };

            return View(viewModel);
        }
    }
}
