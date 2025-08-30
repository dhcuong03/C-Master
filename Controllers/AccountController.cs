using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TestMaster.Models;
using TestMaster.ViewModels;

public class AccountController : Controller
{
    private readonly EmployeeAssessmentContext _context;

    public AccountController(EmployeeAssessmentContext context)
    {
        _context = context;
    }

    #region Login/Logout/AccessDenied Actions
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated)
        {
            // Nếu đã đăng nhập, chuyển thẳng về trang chủ
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ViewData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin.";
            return View();
        }
        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Username == username);
        if (user == null || user.Role == null)
        {
            ViewData["ErrorMessage"] = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return View();
        }

        // Cải tiến bảo mật: Chỉ sử dụng BCrypt.Verify
        bool isPasswordValid = user.PasswordHash != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

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
        var authProperties = new AuthenticationProperties { IsPersistent = false };
        await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

        var loginLog = new AuditLog
        {
            UserId = user.UserId,
            Action = "USER_LOGIN",
            TargetType = "Users",
            TargetId = user.UserId,
            Details = $"Chức vụ '{user.Role.RoleName}' - Người dùng '{user.Username}' đã đăng nhập.",
            LogTime = DateTime.Now
        };
        _context.AuditLogs.Add(loginLog);
        await _context.SaveChangesAsync();

        // === SỬA LỖI: Luôn chuyển hướng về trang Home sau khi đăng nhập ===
        return RedirectToAction("Index", "Home");
        // ===============================================================
    }

    public async Task<IActionResult> Logout()
    {
        var userIdString = User.FindFirstValue("UserId");
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var userName = User.FindFirstValue(ClaimTypes.Name); // Lấy thêm username để log chi tiết hơn

        if (int.TryParse(userIdString, out var userId))
        {
            var logoutLog = new AuditLog
            {
                UserId = userId,
                Action = "USER_LOGOUT",
                TargetType = "Users",
                TargetId = userId,
                Details = $"Chức vụ '{userRole}' - Người dùng '{userName}' đã đăng xuất.",
                LogTime = DateTime.Now
            };
            _context.AuditLogs.Add(logoutLog);
            await _context.SaveChangesAsync();
        }

        await HttpContext.SignOutAsync("MyCookieAuth");
        return RedirectToAction("Login", "Account");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
    #endregion

    #region Forgot/Reset Password Actions
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user == null)
        {
            // Luôn hiển thị thông báo chung để tránh lộ thông tin email có tồn tại hay không
            ViewData["SuccessMessage"] = "Nếu email của bạn tồn tại trong hệ thống, một link khôi phục mật khẩu sẽ được gửi đến.";
            return View();
        }
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        user.PasswordResetToken = token;
        user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync();

        // Trong thực tế, bạn nên gửi link này qua email thay vì hiển thị ra màn hình
        var resetLink = Url.Action("ResetPassword", "Account", new { email = user.Email, token = token }, Request.Scheme);
        ViewData["SuccessMessage"] = "Yêu cầu đã được xử lý. Vui lòng kiểm tra email để lấy link khôi phục mật khẩu.";
        ViewData["ResetLink"] = resetLink; // Tạm thời hiển thị để test
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string email, string token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login");
        }
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.PasswordResetToken == token && u.ResetTokenExpires > DateTime.UtcNow);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Link khôi phục không hợp lệ hoặc đã hết hạn.";
            return RedirectToAction("Login");
        }
        var model = new ResetPasswordViewModel
        {
            Email = email,
            Token = token
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.PasswordResetToken == model.Token && u.ResetTokenExpires > DateTime.UtcNow);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Link khôi phục không hợp lệ hoặc đã hết hạn.";
            return RedirectToAction("Login");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        user.PasswordResetToken = null;
        user.ResetTokenExpires = null;
        user.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        TempData["ResetPasswordSuccess"] = "Mật khẩu của bạn đã được đặt lại thành công. Vui lòng đăng nhập.";
        return RedirectToAction("Login");
    }
    #endregion
}
