using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TestMaster.Controllers // Nhớ đổi namespace cho đúng với project của bạn
{
    // Chỉ những ai có vai trò "Employee" mới được vào trang này
    [Authorize(Roles = "Employee,Manager,Admin,HR")]
    public class EmployeeDashboardController : Controller
    {
        // GET: /EmployeeDashboard/
        public IActionResult Index()
        {
            // Lấy tên đầy đủ của người dùng từ thông tin đã lưu khi đăng nhập
            ViewBag.FullName = User.FindFirst("FullName")?.Value;
            return View();
        }
    }
}
