using System.ComponentModel.DataAnnotations;

namespace ERP.Application.DTOs;

public class InvoiceDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int SaleId { get; set; }
    public string SaleOrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount => Math.Max(0, TotalAmount - PaidAmount);
    public string Status { get; set; } = "Unpaid";
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RecordPaymentDto
{
    [Required]
    [Range(0.01, 10000000)]
    public decimal Amount { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = "Bank Transfer"; // Bank Transfer, Credit Card, Cash, Cheque

    public DateTime PaymentDate { get; set; } = TimeHelper.Now;

    public string Notes { get; set; } = string.Empty;
}


