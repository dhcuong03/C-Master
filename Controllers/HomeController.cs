using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TestMaster.Models;
using Microsoft.AspNetCore.Authorization; // 1. Thêm using này

namespace TestMaster.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // 2. Thêm attribute [Authorize] vào đây
        [Authorize]
        public IActionResult Index()
        {
            // Bây giờ, chỉ những người dùng đã đăng nhập mới có thể truy cập trang này.
            // Nếu chưa đăng nhập, họ sẽ được tự động chuyển đến trang Login.
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
