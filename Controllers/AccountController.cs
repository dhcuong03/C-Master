using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TestMaster.Models;

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
            if (User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("Manager"))
            {
                return RedirectToAction("Index", "AdminDashboard");
            }
            return RedirectToAction("Index", "EmployeeDashboard");
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

        if (user == null)
        {
            ViewData["ErrorMessage"] = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return View();
        }

        bool isPasswordValid;
        try
        {
            isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            isPasswordValid = (user.PasswordHash == password);
        }

        if (!isPasswordValid)
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

        // ✅ Cookie session (mất khi tắt trình duyệt)
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = false // 🔑 quan trọng
        };

        await HttpContext.SignInAsync("MyCookieAuth",
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        // Điều hướng theo Role
        return user.Role.RoleName switch
        {
            "Admin" or "HR" or "Manager" => RedirectToAction("Index", "AdminDashboard"),
            "Employee" => RedirectToAction("Index", "EmployeeDashboard"),
            _ => RedirectToAction("Index", "Home")
        };
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
