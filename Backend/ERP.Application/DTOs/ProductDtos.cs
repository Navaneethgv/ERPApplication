using System.ComponentModel.DataAnnotations;

namespace ERP.Application.DTOs;

public class ProductDto
{
    public int ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public string UnitOfMeasure { get; set; } = "Unit";
    public int ReorderLevel { get; set; }
    public string Status { get; set; } = "Active";
    public int QuantityOnHand { get; set; }
    public int AvailableQuantity { get; set; }
    public bool IsLowStock => QuantityOnHand <= ReorderLevel;
}

public class CreateProductDto
{
    [Required]
    public string SKU { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Range(0.01, 10000000)]
    public decimal UnitPrice { get; set; }

    [Range(0, 10000000)]
    public decimal CostPrice { get; set; }

    public string UnitOfMeasure { get; set; } = "Unit";

    [Range(0, 100000)]
    public int ReorderLevel { get; set; } = 10;

    [Range(0, 100000)]
    public int InitialStock { get; set; } = 0;

    public string Status { get; set; } = "Active";
}

public class UpdateProductDto
{
    [Required]
    public string SKU { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Range(0.01, 10000000)]
    public decimal UnitPrice { get; set; }

    [Range(0, 10000000)]
    public decimal CostPrice { get; set; }

    public string UnitOfMeasure { get; set; } = "Unit";

    [Range(0, 100000)]
    public int ReorderLevel { get; set; }

    public string Status { get; set; } = "Active";
}

