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
    public class SystemConfigurationsController : Controller
    {
        private readonly EmployeeAssessmentContext _context;

        public SystemConfigurationsController(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        // GET: SystemConfigurations
        public async Task<IActionResult> Index()
        {
            return View(await _context.SystemConfigurations.ToListAsync());
        }

        // GET: SystemConfigurations/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var systemConfiguration = await _context.SystemConfigurations
                .FirstOrDefaultAsync(m => m.ConfigKey == id);
            if (systemConfiguration == null)
            {
                return NotFound();
            }

            return View(systemConfiguration);
        }

        // GET: SystemConfigurations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SystemConfigurations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ConfigKey,ConfigValue,Description")] SystemConfiguration systemConfiguration)
        {
            if (ModelState.IsValid)
            {
                _context.Add(systemConfiguration);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(systemConfiguration);
        }

        // GET: SystemConfigurations/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var systemConfiguration = await _context.SystemConfigurations.FindAsync(id);
            if (systemConfiguration == null)
            {
                return NotFound();
            }
            return View(systemConfiguration);
        }

        // POST: SystemConfigurations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ConfigKey,ConfigValue,Description")] SystemConfiguration systemConfiguration)
        {
            if (id != systemConfiguration.ConfigKey)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(systemConfiguration);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SystemConfigurationExists(systemConfiguration.ConfigKey))
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
            return View(systemConfiguration);
        }

        // GET: SystemConfigurations/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var systemConfiguration = await _context.SystemConfigurations
                .FirstOrDefaultAsync(m => m.ConfigKey == id);
            if (systemConfiguration == null)
            {
                return NotFound();
            }

            return View(systemConfiguration);
        }

        // POST: SystemConfigurations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var systemConfiguration = await _context.SystemConfigurations.FindAsync(id);
            if (systemConfiguration != null)
            {
                _context.SystemConfigurations.Remove(systemConfiguration);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SystemConfigurationExists(string id)
        {
            return _context.SystemConfigurations.Any(e => e.ConfigKey == id);
        }
    }
}
