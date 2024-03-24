using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using TradingApp.Models;

namespace TradingApp.Controllers
{
    public class TradingDocumentsController : Controller
    {
        private readonly TradingAppContext _context;

        public TradingDocumentsController(TradingAppContext context)
        {
            _context = context;
        }


        // GET: TradingDocuments
        public async Task<IActionResult> Index()
        {
            var tradingAppContext = _context.TradingDocuments
                .Include(t => t.Invoice)
                .Include(t => t.PurchaseOrder)
                .Include(t => t.Quote)
                .Include(t => t.Rfq)
                .Include(t => t.SalesOder)
                .Include(t => t.Stakeholder)
                .Include(t => t.Organization);
            return View(await tradingAppContext.Where(x=>x.IsActive==true).ToListAsync());
        }

        // GET: TradingDocuments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TradingDocuments == null)
            {
                return NotFound();
            }

            var tradingDocument = await _context.TradingDocuments
                .Include(t => t.TradingDocumentDetails)
                .ThenInclude(t=>t.Item)
                .Include(t => t.Invoice)
                .Include(t => t.PurchaseOrder)
                .Include(t => t.Quote)
                .Include(t => t.Rfq)
                .Include(t => t.SalesOder)
                .Include(t => t.Stakeholder)
                .Include(t => t.Organization)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive == true);
            if (tradingDocument == null)
            {
                return NotFound();
            }

            return View(tradingDocument);
        }

        // GET: TradingDocuments/Create
        public async Task<IActionResult> Create(int? id)
        {
            if (id is not null && id != 0)
            {
                if (_context.TradingDocuments == null)
                {
                    return NotFound();
                }
                var tradingDocument = await _context.TradingDocuments
                .Include(t => t.TradingDocumentDetails)
                .Include(t => t.Invoice)
                .Include(t => t.PurchaseOrder)
                .Include(t => t.Quote)
                .Include(t => t.Rfq)
                .Include(t => t.SalesOder)
                .Include(t => t.Stakeholder)
                .Include(t => t.Organization)

                .FirstOrDefaultAsync(m => m.Id == id);

                if (tradingDocument == null)
                {
                    return NotFound();
                }
                ViewData["InvoiceId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.InvoiceId);
                ViewData["PurchaseOrderId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.PurchaseOrderId);
                ViewData["QuoteId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.QuoteId);
                ViewData["Rfqid"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.Rfqid);
                ViewData["SalesOderId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.SalesOderId);
                ViewData["StakeholderId"] = new SelectList(_context.StakeholderTypes, "Id", "Id", tradingDocument.StakeholderId);
                ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name", tradingDocument.Stakeholder);
                ViewData["Item"] = new SelectList(_context.Items, "Id", "Name");

                return View(tradingDocument);

            }
            ViewData["InvoiceId"] = new SelectList(_context.TradingDocuments, "Id", "Id");
            ViewData["PurchaseOrderId"] = new SelectList(_context.TradingDocuments, "Id", "Id");
            ViewData["QuoteId"] = new SelectList(_context.TradingDocuments, "Id", "Id");
            ViewData["Rfqid"] = new SelectList(_context.TradingDocuments, "Id", "Id");
            ViewData["SalesOderId"] = new SelectList(_context.TradingDocuments, "Id", "Id");
            ViewData["StakeholderId"] = new SelectList(_context.StakeholderTypes, "Id", "Id");
            ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name");
            ViewData["Item"] = new SelectList(_context.Items, "Id", "Name");

            return View();
        }

        // POST: TradingDocuments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id, [Bind("Id,CustomId,DocDate,StakeholderId,BankName,AccountTitle,Rfqid,DueDate,Description,QuoteId,PurchaseOrderId,SalesOderId,InvoiceId,TradingDocumentDetails")] TradingDocument tradingDocument)
        {
            if (!ModelState.IsValid)
            {
                ViewData["InvoiceId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.InvoiceId);
                ViewData["PurchaseOrderId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.PurchaseOrderId);
                ViewData["QuoteId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.QuoteId);
                ViewData["Rfqid"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.Rfqid);
                ViewData["SalesOderId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.SalesOderId);
                ViewData["StakeholderId"] = new SelectList(_context.StakeholderTypes, "Id", "Id", tradingDocument.StakeholderId);
                ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name", tradingDocument.Stakeholder);
                ViewData["Item"] = new SelectList(_context.Items, "Id", "Name");

                return View(tradingDocument);
            }

            foreach (var item in tradingDocument.TradingDocumentDetails)
            {
                item.Amount = item.UnitPrice* item.Quantity;
            }
            
            tradingDocument.SubTotal= tradingDocument.TradingDocumentDetails.Sum(x=>x.Amount);
            tradingDocument.TotalAmount = tradingDocument.TradingDocumentDetails.Sum(x => x.Amount);
            // Get the current user's ID
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get the current user's name
            string userName = User.Identity.Name;

            // Alternatively, you can retrieve the user's name from the claims
            string userNameClaim = User.FindFirstValue(ClaimTypes.Name);

            // Use the userId and userName as needed in your action

            if (id is not null && id != 0)
            {
                if (id != tradingDocument.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        tradingDocument.IsActive = true;
                        tradingDocument.EditedBy = Convert.ToInt32(userId);
                        tradingDocument.EditedOn = DateTime.Now;
                        _context.Update(tradingDocument);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!TradingDocumentExists(tradingDocument.Id))
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
                ViewData["InvoiceId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.InvoiceId);
                ViewData["PurchaseOrderId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.PurchaseOrderId);
                ViewData["QuoteId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.QuoteId);
                ViewData["Rfqid"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.Rfqid);
                ViewData["SalesOderId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.SalesOderId);
                ViewData["StakeholderId"] = new SelectList(_context.StakeholderTypes, "Id", "Id", tradingDocument.StakeholderId);
                ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name", tradingDocument.Stakeholder);
                ViewData["Item"] = new SelectList(_context.Items, "Id", "Name");

                return View(tradingDocument);

            }

            if (ModelState.IsValid)
            {
                tradingDocument.IsActive = true;
                tradingDocument.CreatedBy = Convert.ToInt32(userId);
                tradingDocument.CraetedOn = DateTime.Now;
                _context.Add(tradingDocument);
              var a =  await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
          
            ViewData["InvoiceId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.InvoiceId);
            ViewData["PurchaseOrderId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.PurchaseOrderId);
            ViewData["QuoteId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.QuoteId);
            ViewData["Rfqid"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.Rfqid);
            ViewData["SalesOderId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.SalesOderId);
            ViewData["StakeholderId"] = new SelectList(_context.StakeholderTypes, "Id", "Id", tradingDocument.StakeholderId);
            ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name", tradingDocument.Stakeholder);
            ViewData["Item"] = new SelectList(_context.Items, "Id", "Name");

            return View(tradingDocument);
        }

        // GET: TradingDocuments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TradingDocuments == null)
            {
                return NotFound();
            }

            var tradingDocument = await _context.TradingDocuments.Where(x => x.Id== id && x.IsActive == true).FirstOrDefaultAsync();
            if (tradingDocument == null)
            {
                return NotFound();
            }
            ViewData["InvoiceId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.InvoiceId);
            ViewData["PurchaseOrderId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.PurchaseOrderId);
            ViewData["QuoteId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.QuoteId);
            ViewData["Rfqid"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.Rfqid);
            ViewData["SalesOderId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.SalesOderId);
            ViewData["StakeholderId"] = new SelectList(_context.StakeholderTypes, "Id", "Id", tradingDocument.StakeholderId);
            ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name", tradingDocument.Stakeholder);
            ViewData["Item"] = new SelectList(_context.Items, "Id", "Name");

            return View(tradingDocument);
        }

        // POST: TradingDocuments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomId,DocDate,StakeholderId,BankName,AccountTitle,Rfqid,DueDate,Description,QuoteId,PurchaseOrderId,SalesOderId,InvoiceId,SubTotal,TotalAmount,CreatedBy,CraetedOn,EditedBy,EditedOn")] TradingDocument tradingDocument)
        {
            if (id != tradingDocument.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tradingDocument);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TradingDocumentExists(tradingDocument.Id))
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
            ViewData["InvoiceId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.InvoiceId);
            ViewData["PurchaseOrderId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.PurchaseOrderId);
            ViewData["QuoteId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.QuoteId);
            ViewData["Rfqid"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.Rfqid);
            ViewData["SalesOderId"] = new SelectList(_context.TradingDocuments, "Id", "Id", tradingDocument.SalesOderId);
            ViewData["StakeholderId"] = new SelectList(_context.StakeholderTypes, "Id", "Id", tradingDocument.StakeholderId);
            ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name", tradingDocument.Stakeholder);
            ViewData["Item"] = new SelectList(_context.Items, "Id", "Name");

            return View(tradingDocument);
        }

        // GET: TradingDocuments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TradingDocuments == null)
            {
                return NotFound();
            }

            var tradingDocument = await _context.TradingDocuments
                .Include(t => t.Invoice)
                .Include(t => t.PurchaseOrder)
                .Include(t => t.Quote)
                .Include(t => t.Rfq)
                .Include(t => t.SalesOder)
                .Include(t => t.Stakeholder)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tradingDocument == null)
            {
                return NotFound();
            }

            return View(tradingDocument);
        }

        // POST: TradingDocuments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TradingDocuments == null)
            {
                return Problem("Entity set 'TradingAppContext.TradingDocuments'  is null.");
            }
            var tradingDocument = await _context.TradingDocuments.Include(x=>x.TradingDocumentDetails).FirstOrDefaultAsync(m=>m.Id==id);
            if (tradingDocument != null)
            {
                tradingDocument.IsActive = false;
                foreach (var item in tradingDocument.TradingDocumentDetails) {
                    item.IsActive = false;
                }
                _context.Update(tradingDocument);
                await _context.SaveChangesAsync();
               // _context.TradingDocuments.Remove(tradingDocument);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TradingDocumentExists(int id)
        {
          return (_context.TradingDocuments?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
