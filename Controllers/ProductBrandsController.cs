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
    public class ProductBrandsController : Controller
    {
        private readonly TradingAppContext _context;

        public ProductBrandsController(TradingAppContext context)
        {
            _context = context;
        }

        // GET: ProductBrands
        public async Task<IActionResult> Index()
        {
              return _context.ProductBrands != null ? 
                          View(await _context.ProductBrands.ToListAsync()) :
                          Problem("Entity set 'TradingAppContext.ProductBrands'  is null.");
        }

        // GET: ProductBrands/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ProductBrands == null)
            {
                return NotFound();
            }

            var productBrand = await _context.ProductBrands
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productBrand == null)
            {
                return NotFound();
            }

            return View(productBrand);
        }

        // GET: ProductBrands/Create
        public IActionResult Create(bool IsOnboarding)
        {
            ViewBag.IsOnboarding = IsOnboarding;

            return View();
        }

        // POST: ProductBrands/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,IsActive,CreatedBy,CreatedOn,EditedBy,EditedOn")] ProductBrand productBrand, bool IsOnboarding)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productBrand);
                await _context.SaveChangesAsync();
                if (IsOnboarding)
                {
                    // If it's part of onboarding flow, redirect to create Invoices action
                    return RedirectToAction("Create", "Items", new { IsOnboarding = true });
                }
                else
                {
                    // If it's not part of onboarding flow, redirect to index action
                    return RedirectToAction(nameof(Index));
                }

            }
            return View(productBrand);
        }

        // GET: ProductBrands/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ProductBrands == null)
            {
                return NotFound();
            }

            var productBrand = await _context.ProductBrands.FindAsync(id);
            if (productBrand == null)
            {
                return NotFound();
            }
            return View(productBrand);
        }

        // POST: ProductBrands/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsActive,CreatedBy,CreatedOn,EditedBy,EditedOn")] ProductBrand productBrand)
        {
            if (id != productBrand.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productBrand);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductBrandExists(productBrand.Id))
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
            return View(productBrand);
        }

        // GET: ProductBrands/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ProductBrands == null)
            {
                return NotFound();
            }

            var productBrand = await _context.ProductBrands
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productBrand == null)
            {
                return NotFound();
            }

            return View(productBrand);
        }

        // POST: ProductBrands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ProductBrands == null)
            {
                return Problem("Entity set 'TradingAppContext.ProductBrands'  is null.");
            }
            var productBrand = await _context.ProductBrands.FindAsync(id);
            if (productBrand != null)
            {
                _context.ProductBrands.Remove(productBrand);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductBrandExists(int id)
        {
          return (_context.ProductBrands?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
