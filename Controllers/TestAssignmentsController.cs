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
    public class TestAssignmentsController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public TestAssignmentsController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        // GET: TestAssignments
        public async Task<IActionResult> Index()
        {
            var employeeAssessmentContext = _context.TestAssignments.Include(t => t.AssignedByNavigation).Include(t => t.Department).Include(t => t.Test).Include(t => t.User);
            return View(await employeeAssessmentContext.ToListAsync());
        }

        // GET: TestAssignments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testAssignment = await _context.TestAssignments
                .Include(t => t.AssignedByNavigation)
                .Include(t => t.Department)
                .Include(t => t.Test)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.AssignmentId == id);
            if (testAssignment == null)
            {
                return NotFound();
            }

            return View(testAssignment);
        }

        // GET: TestAssignments/Create
        public IActionResult Create()
        {
            ViewData["AssignedBy"] = new SelectList(_context.Users, "UserId", "UserId");
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentId");
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: TestAssignments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AssignmentId,TestId,UserId,DepartmentId,AssignedBy,AssignedAt,DueDate")] TestAssignment testAssignment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(testAssignment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AssignedBy"] = new SelectList(_context.Users, "UserId", "UserId", testAssignment.AssignedBy);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentId", testAssignment.DepartmentId);
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", testAssignment.TestId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", testAssignment.UserId);
            return View(testAssignment);
        }

        // GET: TestAssignments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testAssignment = await _context.TestAssignments.FindAsync(id);
            if (testAssignment == null)
            {
                return NotFound();
            }
            ViewData["AssignedBy"] = new SelectList(_context.Users, "UserId", "UserId", testAssignment.AssignedBy);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentId", testAssignment.DepartmentId);
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", testAssignment.TestId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", testAssignment.UserId);
            return View(testAssignment);
        }

        // POST: TestAssignments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AssignmentId,TestId,UserId,DepartmentId,AssignedBy,AssignedAt,DueDate")] TestAssignment testAssignment)
        {
            if (id != testAssignment.AssignmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(testAssignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestAssignmentExists(testAssignment.AssignmentId))
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
            ViewData["AssignedBy"] = new SelectList(_context.Users, "UserId", "UserId", testAssignment.AssignedBy);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentId", testAssignment.DepartmentId);
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", testAssignment.TestId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", testAssignment.UserId);
            return View(testAssignment);
        }

        // GET: TestAssignments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testAssignment = await _context.TestAssignments
                .Include(t => t.AssignedByNavigation)
                .Include(t => t.Department)
                .Include(t => t.Test)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.AssignmentId == id);
            if (testAssignment == null)
            {
                return NotFound();
            }

            return View(testAssignment);
        }

        // POST: TestAssignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var testAssignment = await _context.TestAssignments.FindAsync(id);
            if (testAssignment != null)
            {
                _context.TestAssignments.Remove(testAssignment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TestAssignmentExists(int id)
        {
            return _context.TestAssignments.Any(e => e.AssignmentId == id);
        }
    }
}
