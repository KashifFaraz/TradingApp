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
using AutoMapper;
using System.Data.Common;

namespace TradingApp.Controllers
{
    [Authorize]
    //[Route("[controller]/{action=Index}/{id?}")]
    public class InvoicesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly TradingAppContext _context;

        private readonly IMapper _mapper;

        public InvoicesController(TradingAppContext context, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;

        }


        // GET: TradingDocuments
        public async Task<IActionResult> Index([FromQuery] PaginationFilter filters)
        {
            var (invoices, totalRecords) = GetInvoicesWithCount(filters);

            var model = new PaginatedList<Invoice>(invoices.ToList(), totalRecords, filters.PageNumber, filters.PageSize);
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var organization = _context.Organizations.FirstOrDefault(x => x.Id == user.DefaultOrganization);
            ViewBag.Currency = organization.DefaultCurrency;

            // Define common ViewData for dropdown lists
            PopulateInvoiceDropdowns();

            if (id is not null && id != 0)
            {
                var tradingDocument = await _context.Invoices
                    .Include(t => t.InvoiceLines)
                    .ThenInclude(il => il.Item)
                   .ThenInclude(il => il.SaleUnitNavigation)
                    .Include(t => t.Stakeholder)
                    .Include(t => t.Organization)
                    .FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true);

                if (tradingDocument == null)
                {
                    return NotFound();
                }

                // Populate ViewData with trading document specific data
                PopulateTradingDocumentDropdowns(tradingDocument);
                ViewBag.Currency = tradingDocument.Currency;
                return View(tradingDocument);
            }

            return View();
        }






        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id, [Bind("Id, CustomId, DocDate, StakeholderId, BankName, AccountTitle, DueDate,Description,InvoiceId,InvoiceLines,Transaction.CustomId, DocStatusInput")] Invoice entity)
        {
            if (!ModelState.IsValid)
            {
                PopulateViewData(entity);
                return View(entity);
            }

            // Calculate the amount for each invoice line
            foreach (var item in entity.InvoiceLines)
            {
                item.SubTotal = item.UnitPrice * item.Quantity;

                //Discounted Amount
                item.Amount = item.SubTotal - (item.SubTotal * (item.DiscountPercentage / 100));
                item.TaxAmount = item.Amount / 100 * item.TaxPercentage;

            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var defaultOrganizationId = user?.DefaultOrganization; // Perform the null check here
            var organization = _context.Organizations.AsNoTracking().FirstOrDefault(x => x.Id == defaultOrganizationId);

            // Set organization and currency for the invoice
            entity.OrganizationId = user?.DefaultOrganization;
            entity.Currency = organization?.DefaultCurrency;

            // Calculate totals
            entity.SubTotal = entity.InvoiceLines.Sum(x => x.SubTotal);
            entity.TaxAmount = entity.InvoiceLines.Sum(x => x.TaxAmount);
            entity.TotalAmount = entity.InvoiceLines.Sum(x => x.Amount) + entity.TaxAmount;
            entity.UnreconciledAmount = entity.SubTotal;

            // Start a database transaction
            using (var dbTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (id is not null && id != 0)
                    {
                        // Update existing invoice
                        var result = await UpdateInvoiceAsync(id.Value, entity, userId);
                        if (!result.Success)
                        {
                            await dbTransaction.RollbackAsync();
                            ModelState.AddModelError(string.Empty, result.Message);
                            PopulateViewData(entity);
                            return View(entity); // Return to the page with the error message
                        }
                    }
                    else
                    {
                        // Add new invoice
                        var result = await AddInvoiceAsync(entity, userId);
                        if (!result.Success)
                        {
                            await dbTransaction.RollbackAsync();
                            ModelState.AddModelError(string.Empty, result.Message);
                            PopulateViewData(entity);
                            return View(entity); // Return to the page with the error message
                        }
                    }

                    // Save all changes in the transaction (Invoice and journal entries)
                    await _context.SaveChangesAsync();

                    // Commit the transaction only if everything was successful
                    await dbTransaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    // Roll back the transaction in case of an error
                    await dbTransaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message + ex.StackTrace + ex.InnerException);
                    PopulateViewData(entity);
                    throw;
                }
            }

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

        [HttpPut("[controller]/{id}/reset-draft")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetToDraft(int id)
        {
            var entity = await _context.Invoices
                .Include(t => t.Transaction)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true);

            if (entity == null)
            {
                return NotFound();
            }

            // Check if it's already in draft
            if (entity.DocStatus != (byte)DocumentStatus.Finalized)
            {
                return BadRequest(new { Errors = "Only finalized invoices can be reset to draft." });

            }

            if (entity.PaymentReconciliationStatus != (byte)PaymentReconciliationStatus.Unreconciled)
            {


                return BadRequest(new { Errors = "Only Unreconciled invoices can be reset to draft." });
            }

            // Reverse the transaction and related journal entries
            if (entity.TransactionId != null)
            {
                var transaction = await _context.Transactions
                    .Include(t => t.JournalEntries)
                    .FirstOrDefaultAsync(t => t.TransactionId == entity.TransactionId);

                if (transaction != null)
                {
                    // Delete the journal entries associated with the transaction
                    foreach (var journalEntry in transaction.JournalEntries)
                    {
                        journalEntry.IsActive = false;
                        _context.JournalEntries.Update(journalEntry);
                    }

                    // Remove the transaction itself
                    transaction.IsActive = false;
                    _context.Transactions.Update(transaction);
                }
            }

            // Reset the document status and transaction id
            entity.DocStatus = (byte)DocumentStatus.Draft;
            entity.TransactionId = null;
            entity.PaymentReconciliationStatus = (byte)PaymentReconciliationStatus.Unreconciled;
            entity.UnreconciledAmount = entity.TotalAmount;

            // Save changes
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Invoice reset to draft successfully." });

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
            ViewData["Item"] = _context.Items.Include(x => x.SaleUnitNavigation).ToList();
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

        //DB Method
        // Method for adding a new invoice
        private async Task<Result> AddInvoiceAsync(Invoice entity, string userId)
        {


            if (entity.DocStatusInput == (byte)DocumentStatus.Draft)
            {
                entity.DocStatus = (byte)DocumentStatus.Draft;
                entity.PaymentReconciliationStatus = (byte)PaymentReconciliationStatus.Unreconciled;
            }
            else if (entity.DocStatusInput == (byte)DocumentStatus.Finalized)
            {
                // Call the FinalizeInvoiceAsync method
                var result = await FinalizeInvoiceAsync(entity);

                // If the result indicates that the invoice is already finalized, handle it here
                if (!result.Success)
                {
                    return new Result(false, result.Message);
                }
            }

            _context.Add(entity);
            return new Result(true, "Invoice Add successfully");
        }

        // Method for updating an existing invoice
        private async Task<Result> UpdateInvoiceAsync(int id, Invoice entity, string userId)
        {
            var invoice = await _context.Invoices
                .Include(t => t.InvoiceLines)
                .ThenInclude(il => il.Item)
                .ThenInclude(il => il.SaleUnitNavigation)
                .Include(t => t.Stakeholder)
                .Include(t => t.Organization)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true);

            if (invoice == null || invoice.UnreconciledAmount != invoice.TotalAmount)
            {
                new Result(false, "Invoice cannot be changed, payment already exists.");
            }

            if (invoice.DocStatus != (byte)DocumentStatus.Draft)
            {
                new Result(false, "Unable to edit due to the current document status.");
            }

            MapInvoiceParamToInvoiceEntity(entity, invoice);



            _context.Update(invoice);

            if (entity.DocStatusInput == (byte)DocumentStatus.Finalized)
            {
                // Call the FinalizeInvoiceAsync method
                var result = await FinalizeInvoiceAsync(invoice);

                // If the result indicates that the invoice is already finalized, handle it here
                if (!result.Success)
                {
                    return new Result(false, result.Message);
                }
            }
            return new Result(true, "Update invoice successfully");
        }



        private void MapInvoiceParamToInvoiceEntity(Invoice param, Invoice invoice)
        {
            // Manually map each property from the entity to the existing invoice

            invoice.CustomId = param.CustomId;
            invoice.DocDate = param.DocDate;
            invoice.StakeholderId = param.StakeholderId;
            invoice.BankName = param.BankName;
            invoice.AccountTitle = param.AccountTitle;
            invoice.DueDate = param.DueDate;
            invoice.Description = param.Description;
            invoice.SubTotal = param.SubTotal;
            invoice.TotalAmount = param.TotalAmount;
            invoice.UnreconciledAmount = param.UnreconciledAmount;
            invoice.Currency = param.Currency;
            invoice.OrganizationId = param.OrganizationId;





            // Optionally, if handling InvoiceLines, you'll need to decide whether to map or update them manually.
            // This can get complex if there are edits to the individual lines.
            if (param.InvoiceLines != null)
            {
                invoice.InvoiceLines.Clear(); // Clear the existing lines and reassign
                foreach (var line in param.InvoiceLines)
                {
                    invoice.InvoiceLines.Add(line);
                }
            }
        }
        private async Task<Result> FinalizeInvoiceAsync(Invoice entity)
        {
            // Check if the entity is null
            if (entity == null)
            {
                return new Result(false, "Invoice cannot be null.");
            }

            // Check if already finalized
            if (entity.DocStatus == (byte)DocumentStatus.Finalized)
            {
                return new Result(false, "Invoice is already finalized.");
            }

            // Create a new transaction if none exists
            if (entity.TransactionId == null)
            {
                entity.Transaction = new Transaction
                {
                    Type = (byte)TransectionType.Invoice,
                };

                // Add the transaction to context
                _context.Add(entity.Transaction);
                await _context.SaveChangesAsync();
            }

            // Mark invoice as finalized
            entity.DocStatus = (byte)DocumentStatus.Finalized;
            entity.PaymentReconciliationStatus = (byte)PaymentReconciliationStatus.Unreconciled;

            // Update the entity with the Transaction ID (after transaction is saved)
            entity.TransactionId = entity?.Transaction?.TransactionId;

            // Create journal entries based on the finalized invoice
            var accounts = await GetChartOfAccountsAsync();
            var journalEntries = CreateJournalEntries(entity, accounts);

            // Add the journal entries to context
            foreach (var entry in journalEntries)
            {
                _context.Add(entry);
            }

            return new Result(true, "Invoice finalized successfully.");
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
            Amount = invoice.TotalAmount.Value-invoice.SubTotal.Value // Add discount logic if required
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
            ViewData["Item"] = _context.Items.Include(x => x.SaleUnitNavigation).ToList();

        }


        //Repo Methods

        private bool TradingDocumentExists(int id)
        {
            return (_context.Invoices?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        // Method to get total count and paginated invoices
        public (IQueryable<Invoice> Invoices, int TotalRecords) GetInvoicesWithCount(PaginationFilter filters)
        {
            if (_context == null || _context.Invoices == null)
            {
                return (null, 0); // return tuple with null and 0 total records
            }

            // Base query without pagination for filtering and counting
            var baseQuery = _context.Invoices.AsNoTracking()
                .Include(t => t.Stakeholder)
                .Include(t => t.Organization)
                .Where(m => m.IsActive == true);

            // Apply custom filters
            baseQuery = ModelExtension.ApplyFilters(baseQuery, filters);

            // Get the total number of records matching the filter
            int totalRecords = baseQuery.Count();

            // Paginated query with Skip and Take
            var paginatedInvoices = baseQuery
                .OrderBy(i => i.Id) // Sort by the desired field
                .Skip((filters.PageNumber - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .AsNoTracking();

            // Return both paginated data and total count as a tuple
            return (paginatedInvoices, totalRecords);
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
