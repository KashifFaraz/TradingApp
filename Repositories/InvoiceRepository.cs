using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TradingApp.DTOs;
using TradingApp.Models;
using TradingApp.Models.Extension;
using static TradingApp.Utility.Constants;

namespace TradingApp.Repositories
{
    public class InvoiceRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly TradingAppContext _context;

        private readonly IMapper _mapper;
        public InvoiceRepository(TradingAppContext context, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;

        }
        public async Task<Invoice> GetByIDAsync(int id)
        {
            return await _context.Invoices.AsNoTracking()
        .Include(t => t.Transaction)
        .Include(t => t.InvoiceLines.OrderBy(line => line.SortOrder))
        .ThenInclude(t => t.Item)
        .ThenInclude(il => il.SaleUnitNavigation)
        .Include(t => t.SalesOder)
        .Include(t => t.Stakeholder)
        .Include(t => t.Organization)
        .FirstOrDefaultAsync(m => m.Id == id) ?? throw new KeyNotFoundException(Messages.DocumentNotFound);
        }

        public IQueryable<Invoice> GetInvoicesWithCustomer(PaginationFilter filters)
        {

            // Base query without pagination for filtering and counting
            var baseQuery = _context.Invoices.AsNoTracking()
                .Include(t => t.Stakeholder)
                .Include(t => t.Organization).AsQueryable();

            // Apply custom filters
            baseQuery = ModelExtension.ApplyFilters(baseQuery, filters);


            // Paginated query with Skip and Take
            var paginatedInvoices = baseQuery
                .OrderBy(i => i.Id) // Sort by the desired field
                .Skip((filters.PageNumber - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .AsNoTracking();

            return paginatedInvoices;
        }
        public async Task<int> GetInvoicesCount(PaginationFilter filters)
        {

            // Base query without pagination for filtering and counting
            var baseQuery = _context.Invoices.AsNoTracking()
                .Include(t => t.Stakeholder)
                .Include(t => t.Organization).AsQueryable();

            // Apply custom filters
            baseQuery = ModelExtension.ApplyFilters(baseQuery, filters);

            // Get the total number of records matching the filter
            int totalRecords = await baseQuery.CountAsync();

            return totalRecords;
        }
        public async Task<Result> AddInvoiceAsync(Invoice entity, string userId)
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
            return new Result(true, "Documents Add successfully");
        }
        public async Task<Result> UpdateInvoiceAsync(int id, Invoice entity, string userId)
        {
            var invoice = GetByIDAsync(id).Result;

            if (invoice == null || invoice.UnreconciledAmount != invoice.TotalAmount)
            {
                return new Result(false, "Documents cannot be changed, payment already exists.");
            }

            if (invoice.DocStatus != (byte)DocumentStatus.Draft)
            {
                return new Result(false, "Unable to edit due to the current document status.");
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
            return new Result(true, "Update Documents successfully");
        }
        private async Task<Result> FinalizeInvoiceAsync(Invoice entity)
        {
            // Check if the entity is null
            if (entity == null)
            {
                return new Result(false, "Documents cannot be null.");
            }

            // Check if already finalized
            if (entity.DocStatus == (byte)DocumentStatus.Finalized)
            {
                return new Result(false, Messages.InvoiceAlreadyFinalized);
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

            return new Result(true, "Documents finalized successfully.");
        }
        private async Task<Dictionary<string, ChartOfAccount>> GetChartOfAccountsAsync()
        {
            var accountNames = new[] { "Accounts Receivable", "Sales Revenue", "Accounts Payable", "Cost of Goods Sold" };
            var accounts = await _context.ChartOfAccounts
                .Where(a => accountNames.Contains(a.Name))
                .ToDictionaryAsync(a => a.Name);

            return accounts;
        }

        public async Task<Transaction> GetTransactionWithJournalEntriesAsync(int transactionId)
        {
            return await _context.Transactions
                .Include(t => t.JournalEntries)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId) ?? throw new KeyNotFoundException(Messages.DocumentNotFound);
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


        public async Task<(Stakeholder stakeholder, IQueryable<Invoice> invoices, IQueryable<decimal?> UnreconciledPaymentAmount)> GetCustomerDuesAsync(int customerId)
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
                .Where(m => m.StakeholderId == customerId
                    && (m.PaymentReconciliationStatus == (byte)PaymentReconciliationStatus.PartialReconciled
                        || m.PaymentReconciliationStatus == (byte)PaymentReconciliationStatus.Unreconciled));

            var unreconciledPaymentAmount = _context.Receipts
                .Where(r => r.StakeholderId == customerId)
                .Select(x => x.UnreconciledAmount);

            return (stakeholder, invoices, unreconciledPaymentAmount);
        }

        public IQueryable<Invoice> GetDueUnreconcileInvoices(DateTime ToDate, int pageNumber, int pageSize)
        {
            if (_context?.Invoices == null)
            {
                return (Enumerable.Empty<Invoice>().AsQueryable());
            }


            // Fetch the invoices
            var invoices = _context.Invoices
                .Include(t => t.Stakeholder).AsNoTracking()
            .Where(m =>  (m.PaymentReconciliationStatus == (byte)PaymentReconciliationStatus.PartialReconciled
                        || m.PaymentReconciliationStatus == (byte)PaymentReconciliationStatus.Unreconciled)
                    && (m.DueDate < ToDate || m.DueDate == null)
                        )

            .OrderByDescending(x => x.DueDate)
                       .Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var wow = invoices.ToQueryString();

            return invoices;
        }

        public IQueryable<dynamic> GetCustomerDueInvoicesCount(DateTime toDate, int pageNumber, int pageSize)
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
            invoice.Terms = param.Terms;
            invoice.Notes = param.Notes;





            // Optionally, if handling InvoiceLines, you'll need to decide whether to map or update them manually.
            // This can get complex if there are edits to the individual lines.
            if (param.InvoiceLines != null)
            {
                invoice.InvoiceLines.Clear(); // Clear the existing lines and reassign
                int index = 0;
                foreach (var line in param.InvoiceLines)
                {
                    line.SortOrder = index;
                    index++;
                    invoice.InvoiceLines.Add(line);
                }
            }
        }


        public async Task<string?> GetDefaultCurrencyForUserAsync(string userId)
        {
            
            
            var user = await _userManager.FindByIdAsync(userId);
            return _context.Organizations.FirstOrDefault(x => x.Id == user.DefaultOrganization)?.DefaultCurrency;
        }

        public async Task<Organization?> GetDefaultOrganizationAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return await _context.Organizations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == user.DefaultOrganization);
        }


       

    }
    public static class Messages
    {
        public const string DocumentNotFound = "Document not found.";
        public const string InvoiceAlreadyFinalized = "Invoice is already finalized.";
    }
}
