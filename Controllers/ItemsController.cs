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
    public class ItemsController : Controller
    {
        private readonly TradingAppContext _context;

        public ItemsController(TradingAppContext context)
        {
            _context = context;
        }

        // GET: Items
        public async Task<IActionResult> Index()
        {
            var tradingAppContext = _context.Items.Include(i => i.SaleUnitNavigation);
            return View(await tradingAppContext.ToListAsync());
        }

        // GET: Items/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Items == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.SaleUnitNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: Items/Create
        public IActionResult Create(bool IsOnboarding)
        {
            ViewBag.IsOnboarding = IsOnboarding;

            ViewData["Id"] = new SelectList(_context.MeasureUnits, "Id", "Name");
            ViewData["ProductCategory"] = new SelectList(_context.ProductCategories, "Id", "Name");
            ViewData["ProductBrand"] = new SelectList(_context.ProductBrands, "Id", "Name");
            return View();  
        }

        // POST: Items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,SaleUnit")] Item item, bool IsOnboarding)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                if (IsOnboarding)
                {
                    // If it's part of onboarding flow, redirect to create Invoices action
                    return RedirectToAction("Create", "Invoices", new {id =0});
                }
                else
                {
                    // If it's not part of onboarding flow, redirect to index action
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewBag.IsOnboarding = IsOnboarding;

            ViewData["Id"] = new SelectList(_context.MeasureUnits, "Id", "Name", item.Id);
            ViewData["ProductCategory"] = new SelectList(_context.ProductCategories, "Id", "Name");
            ViewData["ProductBrand"] = new SelectList(_context.ProductBrands, "Id", "Name");
            return View(item);
        }

        // GET: Items/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Items == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.MeasureUnits, "Id", "Name", item.Id);
            ViewData["ProductCategory"] = new SelectList(_context.ProductCategories, "Id", "Name");
            ViewData["ProductBrand"] = new SelectList(_context.ProductBrands, "Id", "Name");
            return View(item);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CreatedBy,CraetedOn,EditedBy,EditedOn,SaleUnit")] Item item)
        {
            if (id != item.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.Id))
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
            ViewData["Id"] = new SelectList(_context.MeasureUnits, "Id", "Name", item.Id);
            ViewData["ProductCategory"] = new SelectList(_context.ProductCategories, "Id", "Name");
            ViewData["ProductBrand"] = new SelectList(_context.ProductBrands, "Id", "Name");
            return View(item);
        }

        // GET: Items/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Items == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.SaleUnitNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Items == null)
            {
                return Problem("Entity set 'TradingAppContext.Items'  is null.");
            }
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItemExists(int id)
        {
          return (_context.Items?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
