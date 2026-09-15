namespace ERP.Domain.Entities;

public class Product
{
    public int ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public string UnitOfMeasure { get; set; } = "Unit";
    public int ReorderLevel { get; set; } = 10;
    public RecordStatus Status { get; set; } = RecordStatus.Active;
}

