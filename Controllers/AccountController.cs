using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TestMaster.Models; // Thay YourProjectName

public class AccountController : Controller
{
    private readonly EmployeeAssessmentContext _context;

    public AccountController(EmployeeAssessmentContext context)
    {
        _context = context;
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ViewData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin.";
            return View();
        }

        var user = await _context.Users
                                 .Include(u => u.Role)
                                 .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null || user.PasswordHash != password)
        {
            ViewData["ErrorMessage"] = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return View();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("FullName", user.FullName),
            new Claim(ClaimTypes.Role, user.Role.RoleName),
            new Claim("UserId", user.UserId.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
        var authProperties = new AuthenticationProperties { };

        await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

        // Trong hàm Login, tìm đến đoạn switch
        switch (user.Role.RoleName)
        {
            case "Admin":
            case "HR":
            case "Manager":
                // THAY ĐỔI Ở ĐÂY
                return RedirectToAction("Index", "AdminDashboard");
            case "Employee":
                return RedirectToAction("Index", "EmployeeDashboard");
            default:
                return RedirectToAction("Index", "Home");
        }
    }

    // GET: /Account/Logout
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("MyCookieAuth");
        return RedirectToAction("Login", "Account");
    }

    // GET: /Account/AccessDenied
    public IActionResult AccessDenied()
    {
        return View();
    }
}
