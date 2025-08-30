using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestMaster.Models;

namespace TestMaster.Controllers
{
    [Authorize]
    public class FeedbacksController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public FeedbacksController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        // ===============================================================
        // PHẦN DÀNH CHO ADMIN / HR (CRUD đầy đủ)
        // ===============================================================

        // GET: Feedbacks
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Index()
        {
            var feedbacks = _context.Feedbacks
                .Include(f => f.Test)
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt);
            return View(await feedbacks.ToListAsync());
        }

        // GET: Feedbacks/Details/5
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var feedback = await _context.Feedbacks
                .Include(f => f.Test)
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.FeedbackId == id);

            if (feedback == null) return NotFound();

            return View(feedback);
        }

        // GET: Feedbacks/Create
        [Authorize(Roles = "Admin,HR")]
        public IActionResult Create()
        {
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "Title");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName");
            return View();
        }

        // POST: Feedbacks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Create([Bind("UserId,TestId,Content,Status")] Feedback feedback)
        {
            // Gán giá trị mặc định
            feedback.CreatedAt = DateTime.Now;

            // Bỏ qua kiểm tra ModelState cho các thuộc tính liên kết
            ModelState.Remove("User");
            ModelState.Remove("Test");

            if (ModelState.IsValid)
            {
                _context.Add(feedback);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "Title", feedback.TestId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", feedback.UserId);
            return View(feedback);
        }

        // GET: Feedbacks/Edit/5
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null) return NotFound();

            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "Title", feedback.TestId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", feedback.UserId);
            return View(feedback);
        }

        // POST: Feedbacks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Edit(int id, [Bind("FeedbackId,UserId,TestId,Content,CreatedAt,Status")] Feedback feedback)
        {
            if (id != feedback.FeedbackId) return NotFound();

            ModelState.Remove("User");
            ModelState.Remove("Test");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(feedback);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeedbackExists(feedback.FeedbackId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "Title", feedback.TestId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", feedback.UserId);
            return View(feedback);
        }

        // GET: Feedbacks/Delete/5
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var feedback = await _context.Feedbacks
                .Include(f => f.Test)
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.FeedbackId == id);

            if (feedback == null) return NotFound();

            return View(feedback);
        }

        // POST: Feedbacks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FeedbackExists(int id)
        {
            return _context.Feedbacks.Any(e => e.FeedbackId == id);
        }

        // ===============================================================
        // PHẦN DÀNH CHO NHÂN VIÊN GỬI GÓP Ý (Giữ nguyên)
        // ===============================================================

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] FeedbackSubmitViewModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Content))
            {
                return Json(new { success = false, message = "Nội dung góp ý không được để trống." });
            }

            var userIdString = User.FindFirst("UserId")?.Value;
            if (userIdString == null)
            {
                return Json(new { success = false, message = "Không thể xác thực người dùng." });
            }

            var feedback = new Feedback
            {
                Content = model.Content,
                TestId = model.TestId,
                UserId = int.Parse(userIdString),
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cảm ơn bạn đã gửi góp ý!" });
        }
    }

    public class FeedbackSubmitViewModel
    {
        public int TestId { get; set; }
        public string Content { get; set; }
    }
}
