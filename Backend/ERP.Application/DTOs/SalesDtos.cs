using System.ComponentModel.DataAnnotations;
using ERP.Domain.Entities;
using ERP.Domain.Enums;

namespace ERP.Application.DTOs;

public class SalesItemDto
{
    public int SalesItemId { get; set; }
    public int SaleId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public class SaleDto
{
    public int SaleId { get; set; }
    public string SaleOrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Draft";
    public string Notes { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<SalesItemDto> Items { get; set; } = new();
}

public class CreateSalesItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, 100000)]
    public int Quantity { get; set; }

    [Range(0.01, 10000000)]
    public decimal UnitPrice { get; set; } // If 0, service will populate with current Product.UnitPrice
}

public class CreateSaleDto
{
    public int? CustomerId { get; set; } // Required for Admin/Employee; for Customer role, determined from token

    public DateTime OrderDate { get; set; } = TimeHelper.Today;
    public DateTime? DeliveryDate { get; set; }

    public string Notes { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required in the sales order.")]
    public List<CreateSalesItemDto> Items { get; set; } = new();
}

public class UpdateSaleStatusDto
{
    [Required]
    public SaleStatus Status { get; set; } // Confirmed, Fulfilled, Cancelled
    public string? Notes { get; set; }
}


