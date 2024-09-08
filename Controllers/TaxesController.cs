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
    public class TaxesController : Controller
    {
        private readonly TradingAppContext _context;

        public TaxesController(TradingAppContext context)
        {
            _context = context;
        }

        // GET: Taxes
        public async Task<IActionResult> Index()
        {
            return _context.Taxes != null ?
                        View(await _context.Taxes.ToListAsync()) :
                        Problem("Entity set 'TradingAppContext.Taxes'  is null.");
        }

        // GET: Taxes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Taxes == null)
            {
                return NotFound();
            }

            var tax = await _context.Taxes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tax == null)
            {
                return NotFound();
            }

            return View(tax);
        }

        // GET: Taxes/Create
        public async Task<IActionResult> Create(int? id)
        {
            if (id != null)
            {
                var taxes = await _context.Taxes.FirstOrDefaultAsync(x => x.Id == id);
                if (taxes == null)
                {
                    return NotFound();

                }
                return View(taxes);
            }
            return View();
        }

        // POST: Taxes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id, [Bind("Id,Name,Description,ComputationType,Value,CreatedBy,CreatedOn,EditedBy,EditedOn")] Tax tax)
        {
            if (ModelState.IsValid)
            {
               

                if (id is not null && id != 0)
                {
                    //Update
                    var oldTax = await _context.Taxes.FirstOrDefaultAsync(x => x.Id == id);
                    if (oldTax == null)
                    {
                        return NotFound();

                    }
                    oldTax.Name = tax.Name;
                    oldTax.ComputationType = tax.ComputationType;
                    oldTax.Value = tax.Value;
                    oldTax.Description = tax.Description;

                    _context.Update(oldTax);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    //insert
                    _context.Add(tax);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            
            }
            ViewData["Item"] = _context.Items.ToList();
            return View(tax);
        }

        // GET: Taxes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Taxes == null)
            {
                return NotFound();
            }

            var tax = await _context.Taxes.FindAsync(id);
            if (tax == null)
            {
                return NotFound();
            }
            return View(tax);
        }

        // POST: Taxes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ComputationType,Value,CreatedBy,CreatedOn,EditedBy,EditedOn")] Tax tax)
        {
            if (id != tax.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tax);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaxExists(tax.Id))
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
            return View(tax);
        }

        // GET: Taxes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Taxes == null)
            {
                return NotFound();
            }

            var tax = await _context.Taxes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tax == null)
            {
                return NotFound();
            }

            return View(tax);
        }

        // POST: Taxes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Taxes == null)
            {
                return Problem("Entity set 'TradingAppContext.Taxes'  is null.");
            }
            var tax = await _context.Taxes.FindAsync(id);
            if (tax != null)
            {
                _context.Taxes.Remove(tax);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaxExists(int id)
        {
            return (_context.Taxes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
