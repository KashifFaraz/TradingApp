using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradingApp.Models;

namespace TradingApp.Controllers
{
    public class StakeholderTypesController : Controller
    {
        private readonly TradingAppContext _context;

        public StakeholderTypesController(TradingAppContext context)
        {
            _context = context;
        }

        // GET: StakeholderTypes
        public async Task<IActionResult> Index()
        {
              return _context.StakeholderTypes != null ? 
                          View(await _context.StakeholderTypes.ToListAsync()) :
                          Problem("Entity set 'TradingAppContext.StakeholderTypes'  is null.");
        }

        // GET: StakeholderTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.StakeholderTypes == null)
            {
                return NotFound();
            }

            var stakeholderType = await _context.StakeholderTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stakeholderType == null)
            {
                return NotFound();
            }

            return View(stakeholderType);
        }

        // GET: StakeholderTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: StakeholderTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] StakeholderType stakeholderType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stakeholderType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(stakeholderType);
        }

        // GET: StakeholderTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.StakeholderTypes == null)
            {
                return NotFound();
            }

            var stakeholderType = await _context.StakeholderTypes.FindAsync(id);
            if (stakeholderType == null)
            {
                return NotFound();
            }
            return View(stakeholderType);
        }

        // POST: StakeholderTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] StakeholderType stakeholderType)
        {
            if (id != stakeholderType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stakeholderType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StakeholderTypeExists(stakeholderType.Id))
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
            return View(stakeholderType);
        }

        // GET: StakeholderTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.StakeholderTypes == null)
            {
                return NotFound();
            }

            var stakeholderType = await _context.StakeholderTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stakeholderType == null)
            {
                return NotFound();
            }

            return View(stakeholderType);
        }

        // POST: StakeholderTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.StakeholderTypes == null)
            {
                return Problem("Entity set 'TradingAppContext.StakeholderTypes'  is null.");
            }
            var stakeholderType = await _context.StakeholderTypes.FindAsync(id);
            if (stakeholderType != null)
            {
                _context.StakeholderTypes.Remove(stakeholderType);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StakeholderTypeExists(int id)
        {
          return (_context.StakeholderTypes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
