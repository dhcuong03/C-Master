using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestMaster.Models;

namespace TestMaster.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ người dùng có vai trò "Admin" mới được truy cập
    public class SettingsController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public SettingsController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        // GET: /Settings
        // Trang chính của Cấu hình, chứa các link điều hướng.
        public IActionResult Index()
        {
            return View();
        }

        #region Audit Log Actions

        // GET: /Settings/AuditLog
        // Hiển thị trang Lịch sử hoạt động (Audit Log).
        public async Task<IActionResult> AuditLog()
        {
            var auditLogs = await _context.AuditLogs
                .Include(a => a.User) // Lấy thông tin người dùng liên quan
                .OrderByDescending(a => a.LogTime) // Sắp xếp theo thời gian mới nhất
                .Take(200) // Giới hạn 200 log gần nhất để tối ưu hiệu năng
                .ToListAsync();

            return View(auditLogs);
        }

        // POST: /Settings/DeleteAuditLog/5
        // Xóa một mục log cụ thể dựa vào ID.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAuditLog(int id)
        {
            // Tìm bản ghi log theo id được cung cấp
            var auditLogToDelete = await _context.AuditLogs.FindAsync(id);

            if (auditLogToDelete == null)
            {
                // Nếu không tìm thấy, trả về lỗi Not Found
                TempData["ErrorMessage"] = "Không tìm thấy mục log để xóa.";
                return RedirectToAction(nameof(AuditLog));
            }

            try
            {
                _context.AuditLogs.Remove(auditLogToDelete);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa mục log thành công.";
            }
            catch (DbUpdateException)
            {
                // Ghi lại log lỗi nếu cần thiết trong môi trường production
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa mục log.";
            }

            // Chuyển hướng người dùng trở lại trang danh sách log
            return RedirectToAction(nameof(AuditLog));
        }


        // POST: /Settings/ClearAuditLog
        // Xóa toàn bộ lịch sử hoạt động.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAuditLog()
        {
            try
            {
                // Sử dụng SQL thuần để chạy lệnh DELETE.
                // Cách này hiệu quả hơn nhiều so với việc tải tất cả log vào bộ nhớ rồi xóa.
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM AuditLogs");

                TempData["SuccessMessage"] = "Đã xóa thành công toàn bộ lịch sử hoạt động.";
            }
            catch (Exception)
            {
                // Ghi lại log lỗi nếu cần thiết
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi dọn dẹp lịch sử hoạt động.";
            }

            return RedirectToAction(nameof(AuditLog));
        }

        #endregion

        #region System Configuration Actions

        // GET: /Settings/SystemConfiguration
        // Trang quản lý các Cấu hình hệ thống.
        public async Task<IActionResult> SystemConfiguration()
        {
            var configs = await _context.SystemConfigurations.OrderBy(c => c.ConfigKey).ToListAsync();
            return View(configs);
        }

        // POST: /Settings/SystemConfiguration
        // Cập nhật các Cấu hình hệ thống.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SystemConfiguration(List<SystemConfiguration> configs)
        {
            if (ModelState.IsValid)
            {
                foreach (var config in configs)
                {
                    _context.Entry(config).State = EntityState.Modified;
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cấu hình đã được cập nhật thành công.";
                return RedirectToAction(nameof(SystemConfiguration));
            }
            return View(configs);
        }

        #endregion
    }
}
