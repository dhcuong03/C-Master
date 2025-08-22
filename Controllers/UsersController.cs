using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestMaster.Models; // Sửa lại namespace cho đúng với project của bạn
using Microsoft.AspNetCore.Authorization;

namespace TestMaster.Controllers // Sửa lại namespace cho đúng với project của bạn
{
    [Authorize(Roles = "Admin,HR")]
    public class UsersController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        // ĐÃ SỬA LẠI TÊN CONTEXT CHO ĐÚNG
        public UsersController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var employeeAssessmentContext = _context.Users.Include(u => u.Department).Include(u => u.Level).Include(u => u.Role);
            return View(await employeeAssessmentContext.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Level)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            // Tải dữ liệu cho dropdown, hiển thị tên thay vì ID
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
            ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName");
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName");
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,PasswordHash,FullName,Email,RoleId,DepartmentId,LevelId")] User user)
        {
            // Bỏ qua validate cho các đối tượng liên quan để tránh lỗi
            ModelState.Remove("Role");
            ModelState.Remove("Department");
            ModelState.Remove("Level");

            if (ModelState.IsValid)
            {
                // LƯU Ý: Cần hash mật khẩu ở đây trong thực tế
                // user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Nếu model không hợp lệ, tải lại dữ liệu dropdown
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", user.DepartmentId);
            ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName", user.LevelId);
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", user.DepartmentId);
            ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName", user.LevelId);
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Username,PasswordHash,FullName,Email,RoleId,DepartmentId,LevelId")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            ModelState.Remove("Role");
            ModelState.Remove("Department");
            ModelState.Remove("Level");

            if (ModelState.IsValid)
            {
                try
                {
                    // LƯU Ý: Cần có logic xử lý việc có thay đổi mật khẩu hay không
                    // Ví dụ: chỉ cập nhật mật khẩu nếu người dùng nhập mật khẩu mới.
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
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
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", user.DepartmentId);
            ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName", user.LevelId);
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Level)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
