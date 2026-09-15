using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

public class Purchase
{
    public int PurchaseId { get; set; }
    public string PurchaseNumber { get; set; } = string.Empty; // e.g. PO-2026-0001
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; } = TimeHelper.Today;
    public DateTime ExpectedDeliveryDate { get; set; } = TimeHelper.Today.AddDays(7);
    public decimal TotalAmount { get; set; }
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Draft;
    public string Notes { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = TimeHelper.Now;

    public List<PurchaseItem> Items { get; set; } = new();
}

public class PurchaseItem
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

