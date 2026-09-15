using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

public class Inventory
{
    public int InventoryId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity => Math.Max(0, QuantityOnHand - ReservedQuantity);
    public string Location { get; set; } = "Main Warehouse";
    public DateTime LastUpdated { get; set; } = TimeHelper.Now;
}

