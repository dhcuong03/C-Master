using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TestMaster.Models;
using TestMaster.ViewModels; // Giả định bạn sẽ tạo các ViewModel trong thư mục này

namespace TestMaster.Controllers
{
    // Chỉ người dùng có vai trò Admin hoặc HR mới được truy cập controller này
    [Authorize(Roles = "Admin,HR")]
    public class HRDashboardController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public HRDashboardController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        // GET: /HRDashboard
        // Trang chủ của HR Dashboard
        public async Task<IActionResult> Index()
        {
            ViewData["EmployeeCount"] = await _context.Users.CountAsync(u => u.Role.RoleName != "Admin");
            ViewData["DepartmentCount"] = await _context.Departments.CountAsync();
            ViewData["TestCount"] = await _context.Tests.CountAsync();
            return View();
        }

        #region User Management Actions

        // GET: /HRDashboard/ManageUsers
        // Trang quản lý danh sách người dùng (nhân viên & quản lý)
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users
                                      .Include(u => u.Role)
                                      .Include(u => u.Department)
                                      .Where(u => u.Role.RoleName != "Admin")
                                      .OrderBy(u => u.FullName)
                                      .ToListAsync();
            return View(users);
        }

        // GET: /HRDashboard/CreateUser
        // Hiển thị form để tạo người dùng mới
        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            // Lấy danh sách Roles (chỉ Employee và Manager) và Departments để hiển thị trong dropdown
            ViewData["Roles"] = new SelectList(await _context.Roles.Where(r => r.RoleName != "Admin").ToListAsync(), "RoleId", "RoleName");
            ViewData["Departments"] = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "DepartmentName");
            return View();
        }

        // POST: /HRDashboard/CreateUser
        // Xử lý việc tạo người dùng mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem username hoặc email đã tồn tại chưa
                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại.");
                }
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                }

                if (ModelState.ErrorCount == 0)
                {
                    var newUser = new User
                    {
                        Username = model.Username,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                        FullName = model.FullName,
                        Email = model.Email,
                        RoleId = model.RoleId,
                        DepartmentId = model.DepartmentId,
                        CreatedAt = System.DateTime.Now,
                        UpdatedAt = System.DateTime.Now
                    };

                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Tạo người dùng mới thành công!";
                    return RedirectToAction(nameof(ManageUsers));
                }
            }

            // Nếu model không hợp lệ, tải lại danh sách dropdown và hiển thị lại form
            ViewData["Roles"] = new SelectList(await _context.Roles.Where(r => r.RoleName != "Admin").ToListAsync(), "RoleId", "RoleName", model.RoleId);
            ViewData["Departments"] = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "DepartmentName", model.DepartmentId);
            return View(model);
        }

        #endregion
    }
}
