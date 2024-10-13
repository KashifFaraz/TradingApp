using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradingApp.Models;

namespace TradingApp.Controllers
{
    [Authorize]
    public class StakeholdersController : Controller
    {
        private readonly TradingAppContext _context;

        public StakeholdersController(TradingAppContext context)
        {
            _context = context;
        }

        // GET: Stakeholders
        public async Task<IActionResult> Index()
        {
              return _context.Stakeholders != null ? 
                          View(await _context.Stakeholders.ToListAsync()) :
                          Problem("Entity set 'TradingAppContext.Stakeholders'  is null.");
        }

        // GET: Stakeholders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Stakeholders == null)
            {
                return NotFound();
            }

            var stakeholder = await _context.Stakeholders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stakeholder == null)
            {
                return NotFound();
            }

            return View(stakeholder);
        }

        // GET: Stakeholders/Create
        public IActionResult Create(bool IsOnboarding)
        {
            ViewBag.IsOnboarding = IsOnboarding;
            ViewData["Id"] = new SelectList(_context.StakeholderTypes, "Id", "Name");
            return View();
        }

        // POST: Stakeholders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Type,CreatedBy,CraetedOn,EditedBy,EditedOn")] Stakeholder stakeholder, bool IsOnboarding)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stakeholder);
                await _context.SaveChangesAsync();
                if (IsOnboarding)
                {
                    // If it's part of onboarding flow, redirect to create Invoices action
                    return RedirectToAction("Create", "ProductCategories", new { IsOnboarding = true });
                }
                else
                {
                    // If it's not part of onboarding flow, redirect to index action
                    return RedirectToAction(nameof(Index));
                }
            }
           
            ViewBag.IsOnboarding = IsOnboarding;

            ViewData["Id"] = new SelectList(_context.StakeholderTypes, "Id", "Name",stakeholder.Type);

            return View(stakeholder);
        }

        // GET: Stakeholders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Stakeholders == null)
            {
                return NotFound();
            }

            var stakeholder = await _context.Stakeholders.FindAsync(id);
            if (stakeholder == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.StakeholderTypes, "Id", "Name", stakeholder.Type);

            return View(stakeholder);
        }

        // POST: Stakeholders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,CreatedBy,CraetedOn,EditedBy,EditedOn")] Stakeholder stakeholder)
        {
            if (id != stakeholder.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stakeholder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StakeholderExists(stakeholder.Id))
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
            ViewData["Id"] = new SelectList(_context.StakeholderTypes, "Id", "Name", stakeholder.Type);
            return View(stakeholder);
        }

        // GET: Stakeholders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Stakeholders == null)
            {
                return NotFound();
            }

            var stakeholder = await _context.Stakeholders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stakeholder == null)
            {
                return NotFound();
            }

            return View(stakeholder);
        }

        // POST: Stakeholders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Stakeholders == null)
            {
                return Problem("Entity set 'TradingAppContext.Stakeholders'  is null.");
            }
            var stakeholder = await _context.Stakeholders.FindAsync(id);
            if (stakeholder != null)
            {
                _context.Stakeholders.Remove(stakeholder);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Search(string searchTerm)
        {
            var stakeholders = _context.Stakeholders
                .Where(s => s.Name.Contains(searchTerm))
                .Select(s => new { s.Id, s.Name })
                .Take(10) // Limit results for performance
                .ToList();

            return Json(stakeholders);
        }

        private bool StakeholderExists(int id)
        {
          return (_context.Stakeholders?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
