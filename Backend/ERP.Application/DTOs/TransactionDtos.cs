using System.ComponentModel.DataAnnotations;
using ERP.Domain.Entities;
using ERP.Domain.Enums;

namespace ERP.Application.DTOs;

public class TransactionDto
{
    public int TransactionId { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Income or Expense
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = "Completed";
    public string Notes { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
}

public class CreateTransactionDto
{
    [Required]
    public ReferenceType ReferenceType { get; set; } = ReferenceType.Other;

    public string ReferenceNumber { get; set; } = string.Empty;

    [Required]
    public TransactionType Type { get; set; } = TransactionType.Expense;

    [Required]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 100000000)]
    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; } = TimeHelper.Now;

    public string PaymentMethod { get; set; } = "Bank Transfer";

    public string Notes { get; set; } = string.Empty;
}


