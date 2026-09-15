using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

public class Invoice
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty; // e.g. INV-2026-0001
    public int SaleId { get; set; }
    public string SaleOrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; } = TimeHelper.Today;
    public DateTime DueDate { get; set; } = TimeHelper.Today.AddDays(30);
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount => Math.Max(0, TotalAmount - PaidAmount);
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = TimeHelper.Now;
}

