using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using TradingApp.DTOs;
using TradingApp.Models;
using TradingApp.Models.Extension;
using TradingApp.Repositories;
using TradingApp.ViewModels;
using static TradingApp.Utility.Constants;

namespace TradingApp.Controllers
{
    [Authorize]
    public class InvoicesController : Controller
    {

        private readonly TradingAppContext _context;


        private readonly InvoiceRepository _repository;

        public InvoicesController(TradingAppContext context, UserManager<ApplicationUser> userManager, IMapper mapper, InvoiceRepository repository)
        {
            _context = context;
            _repository = repository;
        }



        public async Task<IActionResult> Index([FromQuery] PaginationFilter filters)
        {
            var invoices = await _repository.GetInvoicesWithCustomer(filters).ToListAsync();
            var totalRecords = await _repository.GetInvoicesCount(filters);

            var model = new PaginatedList<Invoice>(invoices, totalRecords, filters.PageNumber, filters.PageSize);
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var document = await _repository.GetByIDAsync(id.Value);

            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        
        public async Task<IActionResult> Create(int? id)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Currency = await _repository.GetDefaultCurrencyForUserAsync(userId);

            PopulateDropdowns();

            if (id is not null && id != 0)
            {
                var document = await _repository.GetByIDAsync(id.Value);

                if (document == null)
                {
                    return NotFound();
                }

                PopulateDropdowns(document);
                ViewBag.Currency = document.Currency;
                return View(document);
            }

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id, [Bind("Id, CustomId, DocDate, StakeholderId, BankName, AccountTitle, DueDate,Description,InvoiceId,InvoiceLines,Transaction.CustomId, DocStatusInput, Terms, Notes")] Invoice entity)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(entity);
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

            var organization = await _repository.GetDefaultOrganizationAsync(userId);

            // Set organization and currency for the invoice
            entity.OrganizationId = organization.Id;
            entity.Currency = organization?.DefaultCurrency;

            // Calculate totals
            entity.SubTotal = entity.InvoiceLines.Sum(x => x.SubTotal);
            entity.TaxAmount = entity.InvoiceLines.Sum(x => x.TaxAmount);
            entity.TotalAmount = entity.InvoiceLines.Sum(x => x.Amount) + entity.TaxAmount;
            entity.UnreconciledAmount = entity.TotalAmount;

            // Start a database transaction
            using (var dbTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (id is not null && id != 0)
                    {
                        // Update existing invoice
                        var result = await _repository.UpdateInvoiceAsync(id.Value, entity, userId);
                        if (!result.Success)
                        {
                            await dbTransaction.RollbackAsync();
                            ModelState.AddModelError(string.Empty, result.Message);
                            PopulateDropdowns(entity);
                            return View(entity);
                        }
                    }
                    else
                    {
                        // Add new invoice
                        var result = await _repository.AddInvoiceAsync(entity, userId);
                        if (!result.Success)
                        {

                            await dbTransaction.RollbackAsync();
                            ModelState.AddModelError(string.Empty, result.Message);
                            PopulateDropdowns(entity);




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
                    PopulateDropdowns(entity);
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int? id)
        {
            var document = await _repository.GetByIDAsync(id.Value);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var document = await _repository.GetByIDAsync(id);
            if (document != null)
            {
                document.IsActive = false;
                foreach (var item in document.InvoiceLines)
                {
                    item.IsActive = false;
                }
                _context.Update(document);
                await _context.SaveChangesAsync();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPut("[controller]/{id}/reset-draft")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetToDraft(int id)
        {
            var entity = await _repository.GetByIDAsync(id);

            if (entity == null)
            {
                return NotFound();
            }

            
            if (entity.DocStatus != (byte)DocumentStatus.Finalized)
            {
                return BadRequest(new { Errors = "Finalized document can be reset to draft." });

            }

            if (entity.PaymentReconciliationStatus != (byte)PaymentReconciliationStatus.Unreconciled)
            {


                return BadRequest(new { Errors = "Only Unreconciled document can be reset to draft." });
            }

            // Reverse the transaction and related journal entries
            if (entity.TransactionId != null)
            {
                var transaction = await _repository.GetTransactionWithJournalEntriesAsync(entity.TransactionId.Value);

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

            return Ok(new { Message = "Document reset to draft successfully." });

        }

        [HttpGet("[controller]/Customer/{customerId?}")]
        public async Task<IActionResult> CustomerDues(int? customerId)
        {
            if (customerId != null)
            {
                var result = await _repository.GetCustomerDuesAsync(customerId.Value);

                var stakeholder = result.stakeholder;
                var documents = result.invoices.ToList();


                var viewModel = new CustomerDuesViewModel
                {
                    Stakeholder = result.stakeholder,
                    TotalReceivable = result.invoices.Sum(x => x.UnreconciledAmount ?? 0),
                    Invoices = result.invoices.ToList(),
                    UnreconciledPaymentAmount = result.UnreconciledPaymentAmount.Sum() ?? 0, 
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
            var unreconcileInvoices = await _repository.GetDueUnreconcileInvoices(ToDate.Value, pageNumber, pageSize).ToListAsync();
            return PartialView("DueUnreconcileInvoices", unreconcileInvoices); 


        }
        [HttpGet, ActionName("CustomersDueInvoices")]
        public async Task<IActionResult> CustomersDueInvoices(DateTime? ToDate, int pageNumber = 1, int pageSize = 5)
        {
            if (ToDate == null)
            {
                ToDate = DateTime.Today;
            }
            var unreconcileDocuments = await _repository.GetCustomerDueInvoicesCount(ToDate.Value, pageNumber, pageSize).ToListAsync();
            var viewModel = unreconcileDocuments.Select(i => new CustomerDueInvoicesViewModel
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
        private void PopulateDropdowns(Invoice? document = null)
        {
            ViewData["Stakeholder"] = document == null
                ? new SelectList(_context.Stakeholders, "Id", "Name")
                : new SelectList(_context.Stakeholders, "Id", "Name", document.Stakeholder);

            ViewData["Item"] = _context.Items.Include(x => x.SaleUnitNavigation).ToList();
        }
    }
}
