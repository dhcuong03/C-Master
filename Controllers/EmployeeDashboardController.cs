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

        // GET: /EmployeeDashboard/Index (Trang chính để xem thông tin)
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var userProfile = await _context.Users
                .Include(u => u.Role).Include(u => u.Level).Include(u => u.Department)
                .AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);

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

        // GET: /EmployeeDashboard/EditProfile (Hiển thị form sửa hồ sơ)
        public async Task<IActionResult> EditProfile()
        {
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: /EmployeeDashboard/EditProfile (Xử lý việc lưu thay đổi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile([Bind("UserId,FullName,Email")] User userForm)
        {
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId) || userId != userForm.UserId)
            {
                return Unauthorized();
            }

            var userToUpdate = await _context.Users.FindAsync(userId);
            if (userToUpdate == null) return NotFound();

            // Chỉ cập nhật các trường được phép
            userToUpdate.FullName = userForm.FullName;
            userToUpdate.Email = userForm.Email;
            userToUpdate.UpdatedAt = System.DateTime.Now;

            // Bỏ qua validation cho các trường không có trong form
            ModelState.Remove("PasswordHash");
            ModelState.Remove("Username");

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                    return RedirectToAction(nameof(Index)); // Quay về trang chính sau khi lưu
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Cập nhật thất bại. Email có thể đã tồn tại.";
                }
            }

            return View(userForm); // Nếu lỗi, hiển thị lại form với dữ liệu đã nhập
        }

        // GET: /EmployeeDashboard/ChangePassword
        // Hiển thị form đổi mật khẩu
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /EmployeeDashboard/ChangePassword
        // Xử lý việc đổi mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Dùng BCrypt để kiểm tra mật khẩu cũ có khớp không
            if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash))
            {
                ModelState.AddModelError("OldPassword", "Mật khẩu cũ không chính xác.");
                return View(model);
            }

            // Băm mật khẩu mới và cập nhật
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.UpdatedAt = System.DateTime.Now;

            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
