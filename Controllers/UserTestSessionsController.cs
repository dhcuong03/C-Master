using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestMaster.Models;

namespace TestMaster.Controllers
{
    public class UserTestSessionsController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public UserTestSessionsController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        // GET: UserTestSessions
        public async Task<IActionResult> Index()
        {
            var employeeAssessmentContext = _context.UserTestSessions.Include(u => u.Test).Include(u => u.User);
            return View(await employeeAssessmentContext.ToListAsync());
        }

        // GET: UserTestSessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userTestSession = await _context.UserTestSessions
                .Include(u => u.Test)
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.SessionId == id);
            if (userTestSession == null)
            {
                return NotFound();
            }

            return View(userTestSession);
        }

        // GET: UserTestSessions/Create
        public IActionResult Create()
        {
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: UserTestSessions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SessionId,UserId,TestId,StartTime,EndTime,Status,FinalScore,IsPassed")] UserTestSession userTestSession)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userTestSession);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", userTestSession.TestId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", userTestSession.UserId);
            return View(userTestSession);
        }

        // GET: UserTestSessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userTestSession = await _context.UserTestSessions.FindAsync(id);
            if (userTestSession == null)
            {
                return NotFound();
            }
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", userTestSession.TestId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", userTestSession.UserId);
            return View(userTestSession);
        }

        // POST: UserTestSessions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SessionId,UserId,TestId,StartTime,EndTime,Status,FinalScore,IsPassed")] UserTestSession userTestSession)
        {
            if (id != userTestSession.SessionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userTestSession);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserTestSessionExists(userTestSession.SessionId))
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
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", userTestSession.TestId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", userTestSession.UserId);
            return View(userTestSession);
        }

        // GET: UserTestSessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userTestSession = await _context.UserTestSessions
                .Include(u => u.Test)
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.SessionId == id);
            if (userTestSession == null)
            {
                return NotFound();
            }

            return View(userTestSession);
        }

        // POST: UserTestSessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userTestSession = await _context.UserTestSessions.FindAsync(id);
            if (userTestSession != null)
            {
                _context.UserTestSessions.Remove(userTestSession);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserTestSessionExists(int id)
        {
            return _context.UserTestSessions.Any(e => e.SessionId == id);
        }
    }
}
