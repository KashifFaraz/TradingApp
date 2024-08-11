using System;
using System.Collections.Generic;
using System.Drawing.Printing;
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
using TradingApp.ViewModels;
using static TradingApp.Utility.Constants;

namespace TradingApp.Controllers
{
    [Authorize]
    //[Route("[controller]/{action=Index}/{id?}")]
    public class InvoicesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly TradingAppContext _context;

        public InvoicesController(TradingAppContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }


        // GET: TradingDocuments
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var totalItems = await _context.Invoices.CountAsync();
            var invoices = await GetInvoices(pageNumber,pageSize).ToListAsync();

            var model = new PaginatedList<Invoice>(invoices, totalItems, pageNumber, pageSize);
            return View(model);
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
        public async Task<IActionResult> Create(int? id, [Bind("Id,CustomId,DocDate,StakeholderId,BankName,AccountTitle,Rfqid,DueDate,Description,QuoteId,PurchaseOrderId,SalesOderId,InvoiceId,InvoiceLines,Transaction.CustomId")] Invoice invoice)
        {

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
            invoice.UnreconciledAmount = invoice.InvoiceLines.Sum(x => x.Amount);

            // Use the userId and userName as needed in your action

            if (id is not null && id != 0)
            {
                if (id != invoice.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {

                    var oldInvoice = await _context.Invoices.AsNoTracking()
               .Include(t => t.InvoiceLines)
               .ThenInclude(t => t.Item)
               .Include(t => t.PurchaseOrder)
               .Include(t => t.Quote)
               .Include(t => t.Rfq)
               .Include(t => t.SalesOder)
               .Include(t => t.Stakeholder)
               .Include(t => t.Organization)
               .FirstOrDefaultAsync(m => m.Id == id && m.IsActive == true);

                    if (oldInvoice == null)
                    {
                        return NotFound();
                    }

                    if (oldInvoice.UnreconciledAmount != oldInvoice.TotalAmount)
                    {

                        ModelState.AddModelError(string.Empty, "Invoice Can not be changed, payment already exist");

                        ViewData["PurchaseOrderId"] = new SelectList(_context.Invoices, "Id", "Id", invoice.PurchaseOrderId);
                        ViewData["QuoteId"] = new SelectList(_context.Invoices, "Id", "Id", invoice.QuoteId);
                        ViewData["Rfqid"] = new SelectList(_context.Invoices, "Id", "Id", invoice.Rfqid);
                        ViewData["SalesOderId"] = new SelectList(_context.Invoices, "Id", "Id", invoice.SalesOderId);
                        ViewData["StakeholderId"] = new SelectList(_context.StakeholderTypes, "Id", "Id", invoice.StakeholderId);
                        ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name", invoice.Stakeholder);
                        ViewData["Item"] = _context.Items.ToList();


                        return View(invoice);
                    }
                    try
                    {
                        invoice.CreatedBy = oldInvoice.CreatedBy;
                        invoice.CreatedOn = oldInvoice.CreatedOn;
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
                if (invoice.Transaction is null)
                {
                    invoice.Transaction = new Transaction();
                }
                invoice.Transaction.Type = (byte)TransectionType.Invoice;
                invoice.PaymentReconciliationStatus = (byte)PaymentReconciliationStatus.Unreconciled;
                _context.Add(invoice.Transaction);
                await _context.SaveChangesAsync();
                int transactionId = invoice.Transaction.TransactionId;
                invoice.TransactionId = transactionId;
                var receivableAccount = _context.ChartOfAccounts.SingleOrDefault(a => a.Name == "Accounts Receivable" && a.AccountTypeId == (int)FinanceAccountType.Asset);
                var salesAccount = _context.ChartOfAccounts.SingleOrDefault(a => a.Name == "Sales Revenue" && a.AccountTypeId == (int)FinanceAccountType.Income);
                var taxAccount = _context.ChartOfAccounts.SingleOrDefault(a => a.Name == "Accounts Payable" && a.AccountTypeId == (int)FinanceAccountType.Liability);
                var discountAccount = _context.ChartOfAccounts.SingleOrDefault(a => a.Name == "Cost of Goods Sold" && a.AccountTypeId == (int)FinanceAccountType.Expense);

                // Create journal entries for the invoice
                var debitEntry = new JournalEntry
                {
                    TransactionType = (byte)TransectionType.Invoice,
                    AccountId = receivableAccount.Id,
                    TransactionId = invoice.TransactionId.Value,
                    TransactionLineId = 0,
                    Type = "D",
                    Amount = invoice.TotalAmount.Value
                };

                var creditEntryForSales = new JournalEntry
                {
                    TransactionType = (byte)TransectionType.Invoice,
                    AccountId = salesAccount.Id,
                    TransactionId = invoice.TransactionId.Value,
                    TransactionLineId = 0,
                    Type = "C",
                    Amount = invoice.SubTotal.Value
                };
                var creditEntryForTax = new JournalEntry
                {
                    TransactionType = (byte)TransectionType.Invoice,
                    AccountId = taxAccount.Id,
                    TransactionId = invoice.TransactionId.Value,
                    TransactionLineId = 0,
                    Type = "C",
                    Amount = invoice.TotalAmount.Value - invoice.SubTotal.Value
                };
                var creditEntryForDiscount = new JournalEntry
                {
                    TransactionType = (byte)TransectionType.Invoice,
                    AccountId = discountAccount.Id,
                    TransactionId = invoice.TransactionId.Value,
                    TransactionLineId = 0,
                    Type = "C",
                    Amount = 0
                };

                invoice.IsActive = true;
                invoice.CreatedBy = Convert.ToInt32(userId);
                invoice.CreatedOn = DateTime.Now;
                _context.Add(invoice);
                _context.Add(debitEntry);
                _context.Add(creditEntryForSales);
                _context.Add(creditEntryForTax);
                _context.Add(creditEntryForDiscount);
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
                foreach (var item in tradingDocument.InvoiceLines)
                {
                    item.IsActive = false;
                }
                _context.Update(tradingDocument);
                await _context.SaveChangesAsync();
                // _context.TradingDocuments.Remove(tradingDocument);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("[controller]/Customer/{customerId?}")]
        public async Task<IActionResult> CustomerDues(int? customerId)
        {
            if (customerId != null)
            {
                var result = await GetCustomerDuesAsync(customerId.Value);

                var stakeholder = result.stakeholder;
                var invoices = result.invoices.ToList(); // Execute the query to get the invoices


                var viewModel = new CustomerDuesViewModel
                {
                    Stakeholder = result.stakeholder,
                    TotalReceivable = result.invoices.Sum(x => x.UnreconciledAmount ?? 0),
                    Invoices = result.invoices.ToList(),
                    UnreconciledPaymentAmount = result.UnreconciledPaymentAmount.Sum() ?? 0, // Adjusted to sum the amounts
                };
                viewModel.Invoices.ToList().ForEach(x => x.TotalPaid = (x.TotalAmount ?? 0) - (x.UnreconciledAmount ?? 0));
                return View(viewModel);

            }
            return View();
        }
        [HttpGet, ActionName("DueUnreconcileInvoices")]
        public async Task<IActionResult> DueUnreconcileInvoices(DateTime? ToDate, int pageNumber = 1, int pageSize = 5)
        {
            if (ToDate == null)
            {
                ToDate = DateTime.Today;
            }
            var unreconcileInvoices = await GetDueUnreconcileInvoices(ToDate.Value, pageNumber, pageSize).ToListAsync();
            return PartialView("DueUnreconcileInvoices", unreconcileInvoices); // Ensure the partial view is in the right path


        }
        [HttpGet, ActionName("CustomersDueInvoices")]
        public async Task<IActionResult> CustomersDueInvoices(DateTime? ToDate, int pageNumber = 1, int pageSize = 5)
        {
            if (ToDate == null)
            {
                ToDate = DateTime.Today;
            }
            var unreconcileInvoices = await GetCustomerDueInvoicesCount(ToDate.Value, pageNumber, pageSize).ToListAsync();
            var viewModel = unreconcileInvoices.Select(i => new CustomerDueInvoicesViewModel
            {
                StakeholderId = i.StakeholderId,
                Name = i.Name,
                UnpaidInvoice = i.UnpaidInvoice,
                TotalAmount = i.TotalAmount
            }).ToList();
            return View(viewModel);
        }
        private bool TradingDocumentExists(int id)
        {
            return (_context.Invoices?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private IQueryable<Invoice> GetInvoices(int pageNumber , int pageSize)
        {

            if (_context == null || _context.Invoices == null)
            {
                return null;
            }


            return _context.Invoices.AsNoTracking()
            .Include(t => t.Stakeholder)
            .Include(t => t.Organization)
            .Where(m => m.IsActive == true)
            .OrderBy(i => i.Id) // Sort by the desired field
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking();
        }
        private async Task<(Stakeholder stakeholder, IQueryable<Invoice> invoices, IQueryable<decimal?> UnreconciledPaymentAmount)> GetCustomerDuesAsync(int customerId)
        {
            if (_context?.Invoices == null || _context?.Stakeholders == null)
            {
                return (null, Enumerable.Empty<Invoice>().AsQueryable(), Enumerable.Empty<decimal?>().AsQueryable());
            }

            // Fetch the stakeholder data once
            var stakeholder = await _context.Stakeholders.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == customerId);

            // Fetch the invoices
            var invoices = _context.Invoices.AsNoTracking()
                .Where(m => m.IsActive.Value
                    && m.StakeholderId == customerId
                    && (m.PaymentReconciliationStatus == (byte)PaymentReconciliationStatus.PartialReconciled
                        || m.PaymentReconciliationStatus == (byte)PaymentReconciliationStatus.Unreconciled));

            var unreconciledPaymentAmount = _context.Receipts
                .Where(r => r.StakeholderId == customerId)
                .Select(x => x.UnreconciledAmount);

            return (stakeholder, invoices, unreconciledPaymentAmount);
        }

        private IQueryable<Invoice> GetDueUnreconcileInvoices(DateTime ToDate, int pageNumber, int pageSize)
        {
            if (_context?.Invoices == null)
            {
                return (Enumerable.Empty<Invoice>().AsQueryable());
            }


            // Fetch the invoices
            var invoices = _context.Invoices
                .Include(t => t.Stakeholder).AsNoTracking()
            .Where(m => m.IsActive.Value
                    && (m.PaymentReconciliationStatus == (byte)PaymentReconciliationStatus.PartialReconciled
                        || m.PaymentReconciliationStatus == (byte)PaymentReconciliationStatus.Unreconciled)
                    && (m.DueDate < ToDate || m.DueDate == null)
                        )

            .OrderByDescending(x => x.DueDate)
                       .Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var wow = invoices.ToQueryString();

            return invoices;
        }

        private IQueryable<dynamic> GetCustomerDueInvoicesCount(DateTime toDate, int pageNumber, int pageSize)
        {
            if (_context?.Invoices == null)
            {
                return Enumerable.Empty<dynamic>().AsQueryable();
            }

            var invoices = _context.Invoices
                .Include(t => t.Stakeholder).AsNoTracking()
                .Where(m => m.IsActive.Value
                    && (m.PaymentReconciliationStatus == (byte)PaymentReconciliationStatus.PartialReconciled
                        || m.PaymentReconciliationStatus == (byte)PaymentReconciliationStatus.Unreconciled)
                    && (m.DueDate < toDate || m.DueDate == null))
                .GroupBy(group => new { group.StakeholderId, group.Stakeholder.Name })
                .Select(x => new
                {
                    x.Key.StakeholderId,
                    x.Key.Name,
                    UnpaidInvoice = x.Count(),
                    TotalAmount = x.Sum(y => y.TotalAmount ?? 0)
                })
                .OrderByDescending(x => x.TotalAmount)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var query = invoices.ToQueryString();

            return invoices;
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
                //.Include(t => t.CreatedByNavigation)

                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive == true);
        }



    }
}
