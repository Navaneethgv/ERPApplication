using System.ComponentModel.DataAnnotations;
using ERP.Domain.Entities;
using ERP.Domain.Enums;

namespace ERP.Application.DTOs;

public class InventoryDto
{
    public int InventoryId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity => Math.Max(0, QuantityOnHand - ReservedQuantity);
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal TotalValuation => QuantityOnHand * CostPrice;
    public int ReorderLevel { get; set; }
    public bool IsLowStock => QuantityOnHand <= ReorderLevel;
    public string Location { get; set; } = "Main Warehouse";
    public DateTime LastUpdated { get; set; }
}

public class StockAdjustmentDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    public InventoryAdjustmentType AdjustmentType { get; set; } // StockIn, StockOut, Correction, Damaged, Audit

    [Required]
    public int Quantity { get; set; }

    public string Location { get; set; } = "Main Warehouse";

    [Required]
    public string Reason { get; set; } = string.Empty;
}

