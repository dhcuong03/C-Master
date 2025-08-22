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
    public class AnswerOptionsController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public AnswerOptionsController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        // GET: AnswerOptions
        public async Task<IActionResult> Index()
        {
            var employeeAssessmentContext = _context.AnswerOptions.Include(a => a.Question);
            return View(await employeeAssessmentContext.ToListAsync());
        }

        // GET: AnswerOptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var answerOption = await _context.AnswerOptions
                .Include(a => a.Question)
                .FirstOrDefaultAsync(m => m.OptionId == id);
            if (answerOption == null)
            {
                return NotFound();
            }

            return View(answerOption);
        }

        // GET: AnswerOptions/Create
        public IActionResult Create()
        {
            ViewData["QuestionId"] = new SelectList(_context.Questions, "QuestionId", "QuestionId");
            return View();
        }

        // POST: AnswerOptions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OptionId,QuestionId,OptionText,IsCorrect,MatchId")] AnswerOption answerOption)
        {
            if (ModelState.IsValid)
            {
                _context.Add(answerOption);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["QuestionId"] = new SelectList(_context.Questions, "QuestionId", "QuestionId", answerOption.QuestionId);
            return View(answerOption);
        }

        // GET: AnswerOptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var answerOption = await _context.AnswerOptions.FindAsync(id);
            if (answerOption == null)
            {
                return NotFound();
            }
            ViewData["QuestionId"] = new SelectList(_context.Questions, "QuestionId", "QuestionId", answerOption.QuestionId);
            return View(answerOption);
        }

        // POST: AnswerOptions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OptionId,QuestionId,OptionText,IsCorrect,MatchId")] AnswerOption answerOption)
        {
            if (id != answerOption.OptionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(answerOption);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnswerOptionExists(answerOption.OptionId))
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
            ViewData["QuestionId"] = new SelectList(_context.Questions, "QuestionId", "QuestionId", answerOption.QuestionId);
            return View(answerOption);
        }

        // GET: AnswerOptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var answerOption = await _context.AnswerOptions
                .Include(a => a.Question)
                .FirstOrDefaultAsync(m => m.OptionId == id);
            if (answerOption == null)
            {
                return NotFound();
            }

            return View(answerOption);
        }

        // POST: AnswerOptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var answerOption = await _context.AnswerOptions.FindAsync(id);
            if (answerOption != null)
            {
                _context.AnswerOptions.Remove(answerOption);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnswerOptionExists(int id)
        {
            return _context.AnswerOptions.Any(e => e.OptionId == id);
        }
    }
}
