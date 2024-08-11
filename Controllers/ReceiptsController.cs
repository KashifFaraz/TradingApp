using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TradingApp.Models;
using TradingApp.Utility;
using static TradingApp.Utility.Constants;

namespace TradingApp.Controllers
{
    public class ReceiptsController : Controller
    {
        private readonly TradingAppContext _context;

        public ReceiptsController(TradingAppContext context)
        {
            _context = context;
        }

        // GET: Receipts
        public async Task<IActionResult> Index()
        {
            var tradingAppContext = _context.Receipts.Include(r => r.Stakeholder);
            return View(await tradingAppContext.ToListAsync());
        }

        // GET: Receipts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Receipts == null)
            {
                return NotFound();
            }

            var receipt = await _context.Receipts
                .Include(r => r.Stakeholder)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (receipt == null)
            {
                return NotFound();
            }

            return View(receipt);
        }

        // GET: Receipts/Create
        public IActionResult Create(int? invoiceId)
        {

           ViewBag.InvoiceId = invoiceId;
            ViewData["PaymentMethod"] = PaymentMethod.Cash.ToSelectList();
            ViewData["Stakeholder"] = new SelectList(_context.Stakeholders, "Id", "Name");
            
            return View();
        }

        // POST: Receipts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CustomId,DocDate,StakeholderId,BankName,AccountTitle,DueDate,PaymentMethod,Description,SubTotal,TotalAmount,PaymentReconciliationStatus,InvoiceId,CreatedBy,CreatedOn,EditedBy,EditedOn")] Receipt receipt)
        {
            if (!ModelState.IsValid)
            {
                ViewData["StakeholderId"] = new SelectList(_context.Stakeholders, "Id", "Name", receipt.StakeholderId);
                return View(receipt);
            }

            if (receipt.Transaction == null)
            {
                receipt.Transaction = new Transaction();
            }
            // Saving Receipt
            receipt.Transaction.Type = (byte)TransectionType.Invoice;
            receipt.UnreconciledAmount = receipt.TotalAmount ?? 0;
            _context.Add(receipt);
            await _context.SaveChangesAsync();

            receipt.TransactionId = receipt.Transaction.TransactionId;
            
            
            // Accouting Entry

            var cashAccount = _context.ChartOfAccounts.SingleOrDefault(a => a.Name == "Cash" && a.AccountTypeId == (int)FinanceAccountType.Asset);
            var receivableAccount = _context.ChartOfAccounts.SingleOrDefault(a => a.Name == "Accounts Receivable" && a.AccountTypeId == (int)FinanceAccountType.Asset);

            if (cashAccount == null || receivableAccount == null)
            {
                // Handle error: accounts not found
                ModelState.AddModelError(string.Empty, "Required accounts not found.");
                ViewData["StakeholderId"] = new SelectList(_context.Stakeholders, "Id", "Name", receipt.StakeholderId);
                return View(receipt);
            }

            var journalEntries = new List<JournalEntry>
    {
        new JournalEntry
        {
            TransactionType = (byte)TransectionType.Invoice,
            AccountId = cashAccount.Id,
            TransactionId = receipt.TransactionId.Value,
            TransactionLineId = 0,
            Type = "D",
            Amount = receipt.TotalAmount ?? 0
        },
        new JournalEntry
        {
            TransactionType = (byte)TransectionType.Invoice,
            AccountId = receivableAccount.Id,
            TransactionId = receipt.TransactionId.Value,
            TransactionLineId = 0,
            Type = "C",
            Amount = receipt.TotalAmount ?? 0
        }
    };

            _context.AddRange(journalEntries);
            await _context.SaveChangesAsync();

            var invoice = await _context.Invoices.FindAsync(receipt.InvoiceId);
            if (invoice == null)
            {
                // Handle error: invoice not found
                ModelState.AddModelError(string.Empty, "Invoice not found.");
                ViewData["StakeholderId"] = new SelectList(_context.Stakeholders, "Id", "Name", receipt.StakeholderId);
                return View(receipt);
            }

            decimal reconcileAmount = Math.Min(receipt.TotalAmount ?? 0, invoice.UnreconciledAmount ?? 0);

            var paymentReconciliation = new PaymentReconciliation
            {
                Amount = reconcileAmount,
                PaymentId = receipt.Id,
                TradingDocumentId = receipt.InvoiceId
            };

            _context.Add(paymentReconciliation);
            await _context.SaveChangesAsync();

            invoice.UnreconciledAmount -= reconcileAmount;
            receipt.UnreconciledAmount -= reconcileAmount;

            invoice.PaymentReconciliationStatus = invoice.UnreconciledAmount > 0
                ? (byte)PaymentReconciliationStatus.PartialReconciled
                : (byte)PaymentReconciliationStatus.Reconciled;

            receipt.PaymentReconciliationStatus = receipt.UnreconciledAmount > 0
                ? (byte)PaymentReconciliationStatus.PartialReconciled
                : (byte)PaymentReconciliationStatus.Reconciled;

            _context.Update(receipt);
            _context.Update(invoice);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Receipts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Receipts == null)
            {
                return NotFound();
            }

            var receipt = await _context.Receipts.FindAsync(id);
            if (receipt == null)
            {
                return NotFound();
            }
            ViewData["StakeholderId"] = new SelectList(_context.Stakeholders, "Id", "Id", receipt.StakeholderId);
            return View(receipt);
        }

        // POST: Receipts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomId,DocDate,StakeholderId,BankName,AccountTitle,DueDate,PaymentMethod,Description,SubTotal,TotalAmount,UnreconciledAmount,PaymentReconciliationStatus,CreatedBy,CraetedOn,EditedBy,EditedOn")] Receipt receipt)
        {
            if (id != receipt.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(receipt);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReceiptExists(receipt.Id))
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
            ViewData["StakeholderId"] = new SelectList(_context.Stakeholders, "Id", "Id", receipt.StakeholderId);
            return View(receipt);
        }

        // GET: Receipts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Receipts == null)
            {
                return NotFound();
            }

            var receipt = await _context.Receipts
                .Include(r => r.Stakeholder)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (receipt == null)
            {
                return NotFound();
            }

            return View(receipt);
        }

        // POST: Receipts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Receipts == null)
            {
                return Problem("Entity set 'TradingAppContext.Receipts'  is null.");
            }
            var receipt = await _context.Receipts.FindAsync(id);
            if (receipt != null)
            {
                _context.Receipts.Remove(receipt);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpGet("[controller]/{id}/Reconcile")]
        public async Task<IActionResult> ReconcileInvoice(int? id)
        {
            var receipt = await _context.Receipts
               .FirstOrDefaultAsync(m => m.Id == id);

            receipt.TotalReconciled= (receipt.TotalAmount??0)-(receipt.UnreconciledAmount??0);
            ViewBag.ReceiptId = id;
            ViewBag.Receipt = receipt;

              var tradingDocument = await GetUnreconcileInvoices(1, 50).ToListAsync();


            return View(tradingDocument);
        }
        [HttpPost, ActionName("Reconcile")]
        public async Task<IActionResult> ReconcileInvoice(int ReceiptId, string selectedInvoices)
        {
            // Deserialize the JSON string into a list of invoice reconciliation models
            var selectedInvoiceList = JsonConvert.DeserializeObject<List<InvoiceReconciliationModel>>(selectedInvoices);

            // Fetch the receipt from the database
            var receipt = await _context.Receipts.FindAsync(ReceiptId);

            if (receipt == null)
            {
                
                return Json(new { success = false, message = "Receipt not found." });

            }

            // Calculate the total amount to be reconciled from the selected invoices
            decimal toReconcileAmount = selectedInvoiceList.Sum(i => i.amount);

            // Check if the amount to reconcile matches the receipt's unreconciled amount
            if (toReconcileAmount != receipt.UnreconciledAmount)
            {
                return Json(new { success = false, message = "The total amount to be reconciled does not match the receipt's unreconciled amount." });

            }

            foreach (var item in selectedInvoiceList)
            {
                // Fetch the invoice from the database
                var invoice = await _context.Invoices.AsNoTracking().FirstOrDefaultAsync(m => m.Id == item.invoiceId && m.IsActive == true);
          

                if (invoice == null)
                {
                    return Json(new { success = false, message = $"Invoice with ID {item.invoiceId} not found." });
                }

                // Create a new PaymentReconciliation entry
                var paymentReconciliation = new PaymentReconciliation
                {
                    Amount = item.amount,
                    PaymentId = ReceiptId,
                    TradingDocumentId = item.invoiceId
                };

                // Add the payment reconciliation to the context
                _context.PaymentReconciliations.Add(paymentReconciliation);

                // Update the invoice's unreconciled amount
                invoice.UnreconciledAmount -= item.amount;

                // Update the invoice's reconciliation status
                invoice.PaymentReconciliationStatus = invoice.UnreconciledAmount > 0
                    ? (byte)PaymentReconciliationStatus.PartialReconciled
                    : (byte)PaymentReconciliationStatus.Reconciled;

                // Update the invoice in the context
                _context.Invoices.Update(invoice);
            }

            // Update the receipt's unreconciled amount
            receipt.UnreconciledAmount -= toReconcileAmount;

            // Update the receipt's reconciliation status
            receipt.PaymentReconciliationStatus = receipt.UnreconciledAmount > 0
                ? (byte)PaymentReconciliationStatus.PartialReconciled
                : (byte)PaymentReconciliationStatus.Reconciled;

            // Update the receipt in the context
            _context.Receipts.Update(receipt);

            // Save all changes to the database
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Reconciliation successful." });
        }

        public class InvoiceReconciliationModel
        {
            public int invoiceId { get; set; }
            public decimal amount { get; set; }
        }
        //repo methods
        private bool ReceiptExists(int id)
        {
          return (_context.Receipts?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private IQueryable<Invoice> GetUnreconcileInvoices(int pageNumber, int pageSize)
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
                    )

            .OrderByDescending(x => x.DueDate)
                       .Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var wow = invoices.ToQueryString();

            return invoices;
        }
    }
}
