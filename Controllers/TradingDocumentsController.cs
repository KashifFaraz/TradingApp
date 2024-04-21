using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using TradingApp.Models;
using static TradingApp.Utility.Constants;

namespace TradingApp.Controllers
{
    [Authorize]
    public class TradingDocumentsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly TradingAppContext _context;

        public TradingDocumentsController(TradingAppContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }


        // GET: TradingDocuments
        public async Task<IActionResult> Index()
        {
            var tradingDocument =  await GetTradingDocumentWithDetails().ToListAsync();

            
            return View(tradingDocument);
        }

        // GET: TradingDocuments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Invoices == null)
            {
                return NotFound();
            }

            var tradingDocument = await GetTradingDocumentWithDetailsAsync(id.Value);


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
                if (_context.Invoices == null)
                {
                    return NotFound();
                }
                var tradingDocument = await _context.Invoices
                .Include(t => t.InvoiceLines)
                .Include(t => t.PurchaseOrder)
                .Include(t => t.Quote)
                .Include(t => t.Rfq)
                .Include(t => t.SalesOder)
                .Include(t => t.Stakeholder)
                .Include(t => t.Organization)
                .Where(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();

                if (tradingDocument == null)
                {
                    return NotFound();
                }
                ViewData["PurchaseOrderId"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocument.PurchaseOrderId);
                ViewData["QuoteId"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocument.QuoteId);
                ViewData["Rfqid"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocument.Rfqid);
                ViewData["SalesOderId"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocument.SalesOderId);
                ViewData["StakeholderId"] = new SelectList(_context.StakeholderTypes, "Id", "Id", tradingDocument.StakeholderId);
                ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name", tradingDocument.Stakeholder);
                //ViewData["Item"] = new SelectList(_context.Items, "Id", "Name");
                ViewData["Item"] = _context.Items.ToList();

                return View(tradingDocument);

            }
            ViewData["InvoiceId"] = new SelectList(_context.Invoices, "Id", "Id");
            ViewData["PurchaseOrderId"] = new SelectList(_context.Invoices, "Id", "Id");
            ViewData["QuoteId"] = new SelectList(_context.Invoices, "Id", "Id");
            ViewData["Rfqid"] = new SelectList(_context.Invoices, "Id", "Id");
            ViewData["SalesOderId"] = new SelectList(_context.Invoices, "Id", "Id");
            ViewData["StakeholderId"] = new SelectList(_context.StakeholderTypes, "Id", "Id");
            ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name");
            //ViewData["Item"] = new SelectList(_context.Items, "Id", "Name");
            ViewData["Item"] = _context.Items.ToList();

            return View();
        }

        // POST: TradingDocuments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id, [Bind("Id,CustomId,DocDate,StakeholderId,BankName,AccountTitle,Rfqid,DueDate,Description,QuoteId,PurchaseOrderId,SalesOderId,InvoiceId,InvoiceLines")] Invoice invoice)
        {
            invoice.Transaction.Type= (byte)TransectionType.Invoice;
            invoice.TransactionId = invoice.Transaction.TransactionId;

            if (!ModelState.IsValid)
            {
                ViewData["PurchaseOrderId"] = new SelectList(_context.Invoices, "Id", "Id", invoice.PurchaseOrderId);
                ViewData["QuoteId"] = new SelectList(_context.Invoices, "Id", "Id", invoice.QuoteId);
                ViewData["Rfqid"] = new SelectList(_context.Invoices, "Id", "Id", invoice.Rfqid);
                ViewData["SalesOderId"] = new SelectList(_context.Invoices, "Id", "Id", invoice.SalesOderId);
                ViewData["StakeholderId"] = new SelectList(_context.StakeholderTypes, "Id", "Id", invoice.StakeholderId);
                ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name", invoice.Stakeholder);
                ViewData["Item"] = _context.Items.ToList();


                return View(invoice);
            }

            foreach (var item in invoice.InvoiceLines)
            {
                item.Amount = item.UnitPrice * item.Quantity;
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Retrieve the user from the UserManager
            var user = await _userManager.FindByIdAsync(userId);

            invoice.OrganizationId = user.DefaultOrganization;
            invoice.SubTotal = invoice.InvoiceLines.Sum(x => x.Amount);
            invoice.TotalAmount = invoice.InvoiceLines.Sum(x => x.Amount);


            // Use the userId and userName as needed in your action

            if (id is not null && id != 0)
            {
                if (id != invoice.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {

                    var oldTradingDocument = await _context.Invoices.AsNoTracking()
               .Include(t => t.InvoiceLines)
               .ThenInclude(t => t.Item)
               .Include(t => t.PurchaseOrder)
               .Include(t => t.Quote)
               .Include(t => t.Rfq)
               .Include(t => t.SalesOder)
               .Include(t => t.Stakeholder)
               .Include(t => t.Organization)
               .FirstOrDefaultAsync(m => m.Id == id && m.IsActive == true);

                    if (oldTradingDocument == null)
                    {
                        return NotFound();
                    }


                    try
                    {
                        invoice.CreatedBy = oldTradingDocument.CreatedBy;
                        invoice.CreatedOn = oldTradingDocument.CreatedOn;
                        invoice.IsActive = true;
                        invoice.EditedBy = Convert.ToInt32(userId);
                        invoice.EditedOn = DateTime.Now;
                        _context.Update(invoice);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!TradingDocumentExists(invoice.Id))
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
                ViewData["PurchaseOrderId"] = new SelectList(_context.Invoices, "Id", "Id", invoice.PurchaseOrderId);
                ViewData["QuoteId"] = new SelectList(_context.Invoices, "Id", "Id", invoice.QuoteId);
                ViewData["Rfqid"] = new SelectList(_context.Invoices, "Id", "Id", invoice.Rfqid);
                ViewData["SalesOderId"] = new SelectList(_context.Invoices, "Id", "Id", invoice.SalesOderId);
                ViewData["StakeholderId"] = new SelectList(_context.StakeholderTypes, "Id", "Id", invoice.StakeholderId);
                ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name", invoice.Stakeholder);
                ViewData["Item"] = new SelectList(_context.Items, "Id", "Name");

                return View(invoice);

            }

            if (ModelState.IsValid)
            {
                invoice.IsActive = true;
                invoice.CreatedBy = Convert.ToInt32(userId);
                invoice.CreatedOn = DateTime.Now;
                _context.Add(invoice);
                var a = await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["PurchaseOrderId"] = new SelectList(_context.Invoices, "Id", "Id", invoice.PurchaseOrderId);
            ViewData["QuoteId"] = new SelectList(_context.Invoices, "Id", "Id", invoice.QuoteId);
            ViewData["Rfqid"] = new SelectList(_context.Invoices, "Id", "Id", invoice.Rfqid);
            ViewData["SalesOderId"] = new SelectList(_context.Invoices, "Id", "Id", invoice.SalesOderId);
            ViewData["StakeholderId"] = new SelectList(_context.StakeholderTypes, "Id", "Id", invoice.StakeholderId);
            ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name", invoice.Stakeholder);
            ViewData["Item"] = new SelectList(_context.Items, "Id", "Name");

            return View(invoice);
        }

        // GET: TradingDocuments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Invoices == null)
            {
                return NotFound();
            }

            var tradingDocument = await _context.Invoices.Where(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();
            if (tradingDocument == null)
            {
                return NotFound();
            }
            ViewData["PurchaseOrderId"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocument.PurchaseOrderId);
            ViewData["QuoteId"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocument.QuoteId);
            ViewData["Rfqid"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocument.Rfqid);
            ViewData["SalesOderId"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocument.SalesOderId);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomId,DocDate,StakeholderId,BankName,AccountTitle,Rfqid,DueDate,Description,QuoteId,PurchaseOrderId,SalesOderId,InvoiceId,SubTotal,TotalAmount,CreatedBy,CraetedOn,EditedBy,EditedOn")] Invoice tradingDocument)
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
            ViewData["PurchaseOrderId"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocument.PurchaseOrderId);
            ViewData["QuoteId"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocument.QuoteId);
            ViewData["Rfqid"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocument.Rfqid);
            ViewData["SalesOderId"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocument.SalesOderId);
            ViewData["StakeholderId"] = new SelectList(_context.StakeholderTypes, "Id", "Id", tradingDocument.StakeholderId);
            ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name", tradingDocument.Stakeholder);
            ViewData["Item"] = new SelectList(_context.Items, "Id", "Name");

            return View(tradingDocument);
        }

        // GET: TradingDocuments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Invoices == null)
            {
                return NotFound();
            }

            var tradingDocument = await GetTradingDocumentWithDetailsAsync(id.Value);
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
            if (_context.Invoices == null)
            {
                return Problem("Entity set 'TradingAppContext.TradingDocuments'  is null.");
            }
            var tradingDocument = await _context.Invoices.Include(x => x.InvoiceLines).FirstOrDefaultAsync(m => m.Id == id);
            if (tradingDocument != null)
            {
                tradingDocument.IsActive = false;
                foreach (var item in tradingDocument.InvoiceLines) {
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
            return (_context.Invoices?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private  IQueryable<Invoice> GetTradingDocumentWithDetails()
        {

            if (_context == null || _context.Invoices == null)
            {
                return null;
            }


            return  _context.Invoices.AsNoTracking()
            .Include(t => t.InvoiceLines)
                .ThenInclude(t => t.Item)
            .Include(t => t.PurchaseOrder)
            .Include(t => t.Quote)
            .Include(t => t.Rfq)
            .Include(t => t.SalesOder)
            .Include(t => t.Stakeholder)
            .Include(t => t.Organization)
            .Include(t => t.CreatedByNavigation)

            .Where(m => m.IsActive == true);
                

            

            
        }
        private async Task<Invoice> GetTradingDocumentWithDetailsAsync(int id)
        {

            if (_context == null || _context.Invoices == null)
            {
                return null;
            }

           

            return await _context.Invoices.AsNoTracking()
                .Include(t => t.InvoiceLines)
                    .ThenInclude(t => t.Item)
                .Include(t => t.PurchaseOrder)
                .Include(t => t.Quote)
                .Include(t => t.Rfq)
                .Include(t => t.SalesOder)
                .Include(t => t.Stakeholder)
                .Include(t => t.Organization)
                .Include(t => t.CreatedByNavigation)

                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive == true);
        }
        
    }
}
