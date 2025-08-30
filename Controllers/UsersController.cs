    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using TestMaster.Models;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;
    using System.Linq;

    namespace TestMaster.Controllers
    {
        [Authorize(Roles = "Admin,HR,Manager")]
        public class UsersController : Controller
        {
            private readonly EmployeeAssessmentContext _context;

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
                if (id == null) return NotFound();
                var user = await _context.Users
                    .Include(u => u.Department)
                    .Include(u => u.Level)
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(m => m.UserId == id);
                if (user == null) return NotFound();
                return View(user);
            }

            // GET: Users/Create
            public IActionResult Create()
            {
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
                ModelState.Remove("Role");
                ModelState.Remove("Department");
                ModelState.Remove("Level");

                if (ModelState.IsValid)
                {
                    // Mã hóa mật khẩu khi tạo mới
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", user.DepartmentId);
                ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName", user.LevelId);
                ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);
                return View(user);
            }

            // GET: Users/Edit/5
            public async Task<IActionResult> Edit(int? id)
            {
                if (id == null) return NotFound();
                var user = await _context.Users.FindAsync(id);
                if (user == null) return NotFound();

                // Để trống ô mật khẩu khi edit để tránh hiển thị hash
                user.PasswordHash = "";

                ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", user.DepartmentId);
                ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName", user.LevelId);
                ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);
                return View(user);
            }

            // POST: Users/Edit/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, [Bind("UserId,Username,PasswordHash,FullName,Email,RoleId,DepartmentId,LevelId")] User userFromForm)
            {
                if (id != userFromForm.UserId) return NotFound();

                ModelState.Remove("Role");
                ModelState.Remove("Department");
                ModelState.Remove("Level");

                // Bỏ qua validation cho PasswordHash vì nó có thể để trống
                ModelState.Remove("PasswordHash");

                if (ModelState.IsValid)
                {
                    try
                    {
                        var userToUpdate = await _context.Users.FindAsync(id);
                        if (userToUpdate == null) return NotFound();

                        // Cập nhật các thông tin cơ bản
                        userToUpdate.Username = userFromForm.Username;
                        userToUpdate.FullName = userFromForm.FullName;
                        userToUpdate.Email = userFromForm.Email;
                        userToUpdate.RoleId = userFromForm.RoleId;
                        userToUpdate.DepartmentId = userFromForm.DepartmentId;
                        userToUpdate.LevelId = userFromForm.LevelId;

                        // Chỉ cập nhật mật khẩu nếu admin nhập mật khẩu mới
                        if (!string.IsNullOrEmpty(userFromForm.PasswordHash))
                        {
                            userToUpdate.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userFromForm.PasswordHash);
                        }

                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!UserExists(userFromForm.UserId)) return NotFound();
                        else throw;
                    }
                    return RedirectToAction(nameof(Index));
                }
                ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", userFromForm.DepartmentId);
                ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName", userFromForm.LevelId);
                ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", userFromForm.RoleId);
                return View(userFromForm);
            }

            // GET: Users/Delete/5
            public async Task<IActionResult> Delete(int? id)
            {
                if (id == null) return NotFound();
                var user = await _context.Users
                    .Include(u => u.Department)
                    .Include(u => u.Level)
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(m => m.UserId == id);
                if (user == null) return NotFound();
                return View(user);
            }

            // POST: Users/Delete/5
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null) _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            private bool UserExists(int id)
            {
                return _context.Users.Any(e => e.UserId == id);
            }
        }
    }
