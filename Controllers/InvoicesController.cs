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
using static NuGet.Packaging.PackagingConstants;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static TradingApp.Utility.Constants;
using NuGet.Packaging.Signing;
using TradingApp.DTOs;
using TradingApp.Models.Extension;

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
        public async Task<IActionResult> Index([FromQuery]PaginationFilter filters)
        {
            var totalItems = await _context.Invoices.CountAsync();
            var invoices = await GetInvoices(filters).ToListAsync();

            var model = new PaginatedList<Invoice>(invoices, totalItems, filters.PageNumber, filters.PageSize);
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
            if (_context.Invoices == null)
            {
                return NotFound();
            }

            // Define common ViewData for dropdown lists
            PopulateInvoiceDropdowns();

            if (id is not null && id != 0)
            {
                var tradingDocument = await _context.Invoices
                    .Include(t => t.InvoiceLines)
                    .Include(t => t.PurchaseOrder)
                    .Include(t => t.Quote)
                    .Include(t => t.Rfq)
                    .Include(t => t.SalesOder)
                    .Include(t => t.Stakeholder)
                    .Include(t => t.Organization)
                    .FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true);

                if (tradingDocument == null)
                {
                    return NotFound();
                }

                // Populate ViewData with trading document specific data
                PopulateTradingDocumentDropdowns(tradingDocument);
                return View(tradingDocument);
            }

            return View();
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id, [Bind("Id,CustomId,DocDate,StakeholderId,BankName,AccountTitle,Rfqid,DueDate,Description,QuoteId,PurchaseOrderId,SalesOderId,InvoiceId,InvoiceLines,Transaction.CustomId")] Invoice invoice)
        {
            if (!ModelState.IsValid)
            {
                PopulateViewData(invoice);
                return View(invoice);
            }

            foreach (var item in invoice.InvoiceLines)
            {
                item.Amount = item.UnitPrice * item.Quantity;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            invoice.OrganizationId = user.DefaultOrganization;
            invoice.SubTotal = invoice.InvoiceLines.Sum(x => x.Amount);
            invoice.TotalAmount = invoice.SubTotal;
            invoice.UnreconciledAmount = invoice.SubTotal;

            if (id is not null && id != 0)
            {
                if (id != invoice.Id)
                    return NotFound();

                var oldInvoice = await _context.Invoices.AsNoTracking()
                    .Include(t => t.InvoiceLines)
                    .FirstOrDefaultAsync(m => m.Id == id && m.IsActive.Value);

                if (oldInvoice == null || oldInvoice.UnreconciledAmount != oldInvoice.TotalAmount)
                {
                    ModelState.AddModelError(string.Empty, "Invoice cannot be changed, payment already exists.");
                    PopulateViewData(invoice);
                    return View(invoice);
                }

                invoice.CreatedBy = oldInvoice.CreatedBy;
                invoice.CreatedOn = oldInvoice.CreatedOn;
                invoice.EditedBy = Convert.ToInt32(userId);
                invoice.EditedOn = DateTime.Now;

                try
                {
                    _context.Update(invoice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TradingDocumentExists(invoice.Id))
                        return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            // Create new transaction
            if (invoice.TransactionId == null)
            {
                invoice.Transaction = new Transaction
                {
                    Type = (byte)TransectionType.Invoice,
                };
            }

            invoice.PaymentReconciliationStatus = (byte)PaymentReconciliationStatus.Unreconciled;
            _context.Add(invoice.Transaction);
            await _context.SaveChangesAsync();

            invoice.TransactionId = invoice.Transaction.TransactionId;

            var accounts = await GetChartOfAccountsAsync();

            var journalEntries = CreateJournalEntries(invoice, accounts);
            foreach (var entry in journalEntries)
            {
                _context.Add(entry);
            }

            invoice.IsActive = true;
            invoice.CreatedBy = Convert.ToInt32(userId);
            invoice.CreatedOn = DateTime.Now;

            _context.Add(invoice);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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

        //private methods

        // Helper method to populate common dropdown lists
        private void PopulateInvoiceDropdowns()
        {
            ViewData["InvoiceId"] = new SelectList(_context.Invoices, "Id", "Id");
            ViewData["PurchaseOrderId"] = new SelectList(_context.Invoices, "Id", "Id");
            ViewData["QuoteId"] = new SelectList(_context.Invoices, "Id", "Id");
            ViewData["Rfqid"] = new SelectList(_context.Invoices, "Id", "Id");
            ViewData["SalesOderId"] = new SelectList(_context.Invoices, "Id", "Id");
            ViewData["StakeholderId"] = new SelectList(_context.StakeholderTypes, "Id", "Id");
            ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name");
            ViewData["Item"] = _context.Items.ToList(); // Could be SelectList if needed
        }

        // Helper method to populate dropdowns based on trading document
        private void PopulateTradingDocumentDropdowns(Invoice tradingDocument)
        {
            ViewData["PurchaseOrderId"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocument.PurchaseOrderId);
            ViewData["QuoteId"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocument.QuoteId);
            ViewData["Rfqid"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocument.Rfqid);
            ViewData["SalesOderId"] = new SelectList(_context.Invoices, "Id", "Id", tradingDocument.SalesOderId);
            ViewData["StakeholderId"] = new SelectList(_context.StakeholderTypes, "Id", "Id", tradingDocument.StakeholderId);
            ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name", tradingDocument.Stakeholder);
            ViewData["Item"] = _context.Items.ToList(); // Could be SelectList if needed
        }

        private async Task<Dictionary<string, ChartOfAccount>> GetChartOfAccountsAsync()
        {
            var accountNames = new[] { "Accounts Receivable", "Sales Revenue", "Accounts Payable", "Cost of Goods Sold" };
            var accounts = await _context.ChartOfAccounts
                .Where(a => accountNames.Contains(a.Name))
                .ToDictionaryAsync(a => a.Name);

            return accounts;
        }

        private IEnumerable<JournalEntry> CreateJournalEntries(Invoice invoice, Dictionary<string, ChartOfAccount> accounts)
        {
            return new List<JournalEntry>
    {
        new JournalEntry
        {
            TransactionType = (byte)TransectionType.Invoice,
            AccountId = accounts["Accounts Receivable"].Id,
            TransactionId = invoice.TransactionId.Value,
            Type = "D",
            Amount = invoice.TotalAmount.Value
        },
        new JournalEntry
        {
            TransactionType = (byte)TransectionType.Invoice,
            AccountId = accounts["Sales Revenue"].Id,
            TransactionId = invoice.TransactionId.Value,
            Type = "C",
            Amount = invoice.SubTotal.Value
        },
        new JournalEntry
        {
            TransactionType = (byte)TransectionType.Invoice,
            AccountId = accounts["Accounts Payable"].Id,
            TransactionId = invoice.TransactionId.Value,
            Type = "C",
            Amount = invoice.TotalAmount.Value - invoice.SubTotal.Value
        },
        new JournalEntry
        {
            TransactionType = (byte)TransectionType.Invoice,
            AccountId = accounts["Cost of Goods Sold"].Id,
            TransactionId = invoice.TransactionId.Value,
            Type = "C",
            Amount = 0 // Add discount logic if required
        }
    };
        }

        private void PopulateViewData(Invoice invoice)
        {
            ViewData["PurchaseOrderId"] = new SelectList(_context.Invoices, "Id", "Id", invoice.PurchaseOrderId);
            ViewData["QuoteId"] = new SelectList(_context.Invoices, "Id", "Id", invoice.QuoteId);
            ViewData["Rfqid"] = new SelectList(_context.Invoices, "Id", "Id", invoice.Rfqid);
            ViewData["SalesOderId"] = new SelectList(_context.Invoices, "Id", "Id", invoice.SalesOderId);
            ViewData["StakeholderId"] = new SelectList(_context.StakeholderTypes, "Id", "Id", invoice.StakeholderId);
            ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name", invoice.Stakeholder);
            ViewData["Item"] = _context.Items.ToList();
        }


        //Repo Methods

        private bool TradingDocumentExists(int id)
        {
            return (_context.Invoices?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private IQueryable<Invoice> GetInvoices(PaginationFilter filters)
        {

            if (_context == null || _context.Invoices == null)
            {
                return null;
            }

            

            var invoices = _context.Invoices.AsNoTracking()
            .Include(t => t.Stakeholder)
            .Include(t => t.Organization)
            .Where(m => m.IsActive == true)
            .OrderBy(i => i.Id) // Sort by the desired field
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .AsNoTracking();

            invoices = ModelExtension.ApplyFilters(invoices, filters);
            return invoices;
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
