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
    public class UserAnswersController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public UserAnswersController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        // GET: UserAnswers
        public async Task<IActionResult> Index()
        {
            var employeeAssessmentContext = _context.UserAnswers.Include(u => u.ChosenOption).Include(u => u.GradedByNavigation).Include(u => u.Question).Include(u => u.Session);
            return View(await employeeAssessmentContext.ToListAsync());
        }

        // GET: UserAnswers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userAnswer = await _context.UserAnswers
                .Include(u => u.ChosenOption)
                .Include(u => u.GradedByNavigation)
                .Include(u => u.Question)
                .Include(u => u.Session)
                .FirstOrDefaultAsync(m => m.UserAnswerId == id);
            if (userAnswer == null)
            {
                return NotFound();
            }

            return View(userAnswer);
        }

        // GET: UserAnswers/Create
        public IActionResult Create()
        {
            ViewData["ChosenOptionId"] = new SelectList(_context.AnswerOptions, "OptionId", "OptionId");
            ViewData["GradedBy"] = new SelectList(_context.Users, "UserId", "UserId");
            ViewData["QuestionId"] = new SelectList(_context.Questions, "QuestionId", "QuestionId");
            ViewData["SessionId"] = new SelectList(_context.UserTestSessions, "SessionId", "SessionId");
            return View();
        }

        // POST: UserAnswers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserAnswerId,SessionId,QuestionId,ChosenOptionId,AnswerText,Score,GraderNotes,GradedBy,GradedAt")] UserAnswer userAnswer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userAnswer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ChosenOptionId"] = new SelectList(_context.AnswerOptions, "OptionId", "OptionId", userAnswer.ChosenOptionId);
            ViewData["GradedBy"] = new SelectList(_context.Users, "UserId", "UserId", userAnswer.GradedBy);
            ViewData["QuestionId"] = new SelectList(_context.Questions, "QuestionId", "QuestionId", userAnswer.QuestionId);
            ViewData["SessionId"] = new SelectList(_context.UserTestSessions, "SessionId", "SessionId", userAnswer.SessionId);
            return View(userAnswer);
        }

        // GET: UserAnswers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userAnswer = await _context.UserAnswers.FindAsync(id);
            if (userAnswer == null)
            {
                return NotFound();
            }
            ViewData["ChosenOptionId"] = new SelectList(_context.AnswerOptions, "OptionId", "OptionId", userAnswer.ChosenOptionId);
            ViewData["GradedBy"] = new SelectList(_context.Users, "UserId", "UserId", userAnswer.GradedBy);
            ViewData["QuestionId"] = new SelectList(_context.Questions, "QuestionId", "QuestionId", userAnswer.QuestionId);
            ViewData["SessionId"] = new SelectList(_context.UserTestSessions, "SessionId", "SessionId", userAnswer.SessionId);
            return View(userAnswer);
        }

        // POST: UserAnswers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserAnswerId,SessionId,QuestionId,ChosenOptionId,AnswerText,Score,GraderNotes,GradedBy,GradedAt")] UserAnswer userAnswer)
        {
            if (id != userAnswer.UserAnswerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userAnswer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserAnswerExists(userAnswer.UserAnswerId))
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
            ViewData["ChosenOptionId"] = new SelectList(_context.AnswerOptions, "OptionId", "OptionId", userAnswer.ChosenOptionId);
            ViewData["GradedBy"] = new SelectList(_context.Users, "UserId", "UserId", userAnswer.GradedBy);
            ViewData["QuestionId"] = new SelectList(_context.Questions, "QuestionId", "QuestionId", userAnswer.QuestionId);
            ViewData["SessionId"] = new SelectList(_context.UserTestSessions, "SessionId", "SessionId", userAnswer.SessionId);
            return View(userAnswer);
        }

        // GET: UserAnswers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userAnswer = await _context.UserAnswers
                .Include(u => u.ChosenOption)
                .Include(u => u.GradedByNavigation)
                .Include(u => u.Question)
                .Include(u => u.Session)
                .FirstOrDefaultAsync(m => m.UserAnswerId == id);
            if (userAnswer == null)
            {
                return NotFound();
            }

            return View(userAnswer);
        }

        // POST: UserAnswers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userAnswer = await _context.UserAnswers.FindAsync(id);
            if (userAnswer != null)
            {
                _context.UserAnswers.Remove(userAnswer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserAnswerExists(int id)
        {
            return _context.UserAnswers.Any(e => e.UserAnswerId == id);
        }
    }
}
