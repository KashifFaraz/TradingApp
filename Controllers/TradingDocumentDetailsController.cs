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
    public class TradingDocumentDetailsController : Controller
    {
        private readonly TradingAppContext _context;

        public TradingDocumentDetailsController(TradingAppContext context)
        {
            _context = context;
        }

        // GET: TradingDocumentDetails
        public async Task<IActionResult> Index()
        {
            var tradingAppContext = _context.InvoiceLines.Include(t => t.Item).Include(t => t.Master);
            return View(await tradingAppContext.ToListAsync());
        }

        // GET: TradingDocumentDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.InvoiceLines == null)
            {
                return NotFound();
            }

            var tradingDocumentDetail = await _context.InvoiceLines
                .Include(t => t.Item)
                .Include(t => t.Master)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tradingDocumentDetail == null)
            {
                return NotFound();
            }

            return View(tradingDocumentDetail);
        }

        // GET: TradingDocumentDetails/Create
        public IActionResult Create()
        {
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Name");
            ViewData["MasterId"] = new SelectList(_context.Invoices, "Id", "Name");
            return View();
        }

        // POST: TradingDocumentDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MasterId,Description,ItemId,UnitPrice,Amount,Quantity,CreatedBy,CraetedOn,EditedBy,EditedOn")] InvoiceLine tradingDocumentDetail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tradingDocumentDetail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Id", tradingDocumentDetail.ItemId);
            ViewData["MasterId"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocumentDetail.MasterId);
            return View(tradingDocumentDetail);
        }

        // GET: TradingDocumentDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.InvoiceLines == null)
            {
                return NotFound();
            }

            var tradingDocumentDetail = await _context.InvoiceLines.FindAsync(id);
            if (tradingDocumentDetail == null)
            {
                return NotFound();
            }
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Id", tradingDocumentDetail.ItemId);
            ViewData["MasterId"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocumentDetail.MasterId);
            return View(tradingDocumentDetail);
        }

        // POST: TradingDocumentDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MasterId,Description,ItemId,UnitPrice,Amount,Quantity,CreatedBy,CraetedOn,EditedBy,EditedOn")] InvoiceLine tradingDocumentDetail)
        {
            if (id != tradingDocumentDetail.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tradingDocumentDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TradingDocumentDetailExists(tradingDocumentDetail.Id))
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
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Id", tradingDocumentDetail.ItemId);
            ViewData["MasterId"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocumentDetail.MasterId);
            return View(tradingDocumentDetail);
        }

        // GET: TradingDocumentDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.InvoiceLines == null)
            {
                return NotFound();
            }

            var tradingDocumentDetail = await _context.InvoiceLines
                .Include(t => t.Item)
                .Include(t => t.Master)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tradingDocumentDetail == null)
            {
                return NotFound();
            }

            return View(tradingDocumentDetail);
        }

        // POST: TradingDocumentDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.InvoiceLines == null)
            {
                return Problem("Entity set 'TradingAppContext.TradingDocumentDetails'  is null.");
            }
            var tradingDocumentDetail = await _context.InvoiceLines.FindAsync(id);
            if (tradingDocumentDetail != null)
            {
                _context.InvoiceLines.Remove(tradingDocumentDetail);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TradingDocumentDetailExists(int id)
        {
          return (_context.InvoiceLines?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
