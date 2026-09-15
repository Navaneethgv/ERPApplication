using System.ComponentModel.DataAnnotations;
using ERP.Domain.Entities;
using ERP.Domain.Enums;

namespace ERP.Application.DTOs;

public class PurchaseItemDto
{
    public int PurchaseItemId { get; set; }
    public int PurchaseId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public class PurchaseDto
{
    public int PurchaseId { get; set; }
    public string PurchaseNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Draft";
    public string Notes { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<PurchaseItemDto> Items { get; set; } = new();
}

public class CreatePurchaseItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, 100000)]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, 10000000)]
    public decimal UnitPrice { get; set; }
}

public class CreatePurchaseDto
{
    [Required]
    public int SupplierId { get; set; }

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime ExpectedDeliveryDate { get; set; } = DateTime.UtcNow.Date.AddDays(7);

    public string Notes { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required in the purchase order.")]
    public List<CreatePurchaseItemDto> Items { get; set; } = new();
}

public class UpdatePurchaseStatusDto
{
    [Required]
    public PurchaseStatus Status { get; set; } // Ordered, Received, Cancelled
    public string? Notes { get; set; }
}

