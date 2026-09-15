using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

public class Transaction
{
    public int TransactionId { get; set; }
    public string TransactionNumber { get; set; } = string.Empty; // e.g. TXN-2026-0001
    public ReferenceType ReferenceType { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty; // PO-2026-0001, INV-2026-0001, etc.
    public TransactionType Type { get; set; } // Income or Expense
    public string Category { get; set; } = string.Empty; // Sales Revenue, Inventory Purchase, Customer Payment, Operating Expense, etc.
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; } = TimeHelper.Now;
    public string PaymentMethod { get; set; } = "Bank Transfer"; // Bank Transfer, Credit Card, Cash, Cheque
    public string Status { get; set; } = "Completed"; // Completed, Pending, Failed
    public string Notes { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = "System";
}

