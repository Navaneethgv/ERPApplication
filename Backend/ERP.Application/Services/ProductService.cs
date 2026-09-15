



namespace ERP.Application.Services;
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IInventoryRepository _inventoryRepository;

    public ProductService(IProductRepository productRepository, IInventoryRepository inventoryRepository)
    {
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
    }

    public List<ProductDto> GetAll()
    {
        var products = _productRepository.GetAll();
        var inventory = _inventoryRepository.GetAll().ToDictionary(i => i.ProductId);

        return products.Select(p =>
        {
            var inv = inventory.TryGetValue(p.ProductId, out var item) ? item : null;
            return new ProductDto
            {
                ProductId = p.ProductId,
                SKU = p.SKU,
                Name = p.Name,
                Category = p.Category,
                Description = p.Description,
                UnitPrice = p.UnitPrice,
                CostPrice = p.CostPrice,
                UnitOfMeasure = p.UnitOfMeasure,
                ReorderLevel = p.ReorderLevel,
                Status = p.Status.ToString(),
                QuantityOnHand = inv?.QuantityOnHand ?? 0,
                AvailableQuantity = inv?.AvailableQuantity ?? 0
            };
        }).ToList();
    }

    public ProductDto? GetById(int id)
    {
        var p = _productRepository.GetById(id);
        if (p == null) return null;

        var inv = _inventoryRepository.GetByProductId(id);
        return new ProductDto
        {
            ProductId = p.ProductId,
            SKU = p.SKU,
            Name = p.Name,
            Category = p.Category,
            Description = p.Description,
            UnitPrice = p.UnitPrice,
            CostPrice = p.CostPrice,
            UnitOfMeasure = p.UnitOfMeasure,
            ReorderLevel = p.ReorderLevel,
            Status = p.Status.ToString(),
            QuantityOnHand = inv?.QuantityOnHand ?? 0,
            AvailableQuantity = inv?.AvailableQuantity ?? 0
        };
    }

    public ProductDto Create(CreateProductDto dto)
    {
        var existing = _productRepository.GetBySKU(dto.SKU);
        if (existing != null)
        {
            throw new InvalidOperationException($"Product with SKU '{dto.SKU}' already exists.");
        }

        var product = new Product
        {
            SKU = dto.SKU.Trim().ToUpperInvariant(),
            Name = dto.Name.Trim(),
            Category = dto.Category.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            UnitPrice = dto.UnitPrice,
            CostPrice = dto.CostPrice,
            UnitOfMeasure = string.IsNullOrEmpty(dto.UnitOfMeasure) ? "Unit" : dto.UnitOfMeasure.Trim(),
            ReorderLevel = dto.ReorderLevel,
            Status = Enum.TryParse<RecordStatus>(dto.Status, true, out var st) ? st : RecordStatus.Active
        };

        product = _productRepository.Add(product);

        // Create inventory row
        var inv = new Inventory
        {
            ProductId = product.ProductId,
            ProductName = product.Name,
            SKU = product.SKU,
            QuantityOnHand = dto.InitialStock,
            ReservedQuantity = 0,
            Location = "Main Warehouse",
            LastUpdated = DateTime.UtcNow
        };
        _inventoryRepository.Add(inv);

        return new ProductDto
        {
            ProductId = product.ProductId,
            SKU = product.SKU,
            Name = product.Name,
            Category = product.Category,
            Description = product.Description,
            UnitPrice = product.UnitPrice,
            CostPrice = product.CostPrice,
            UnitOfMeasure = product.UnitOfMeasure,
            ReorderLevel = product.ReorderLevel,
            Status = product.Status.ToString(),
            QuantityOnHand = inv.QuantityOnHand,
            AvailableQuantity = inv.AvailableQuantity
        };
    }

    public ProductDto Update(int id, UpdateProductDto dto)
    {
        var product = _productRepository.GetById(id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        }

        var existingSku = _productRepository.GetBySKU(dto.SKU);
        if (existingSku != null && existingSku.ProductId != id)
        {
            throw new InvalidOperationException($"SKU '{dto.SKU}' is already used by another product.");
        }

        product.SKU = dto.SKU.Trim().ToUpperInvariant();
        product.Name = dto.Name.Trim();
        product.Category = dto.Category.Trim();
        product.Description = dto.Description?.Trim() ?? string.Empty;
        product.UnitPrice = dto.UnitPrice;
        product.CostPrice = dto.CostPrice;
        product.UnitOfMeasure = string.IsNullOrEmpty(dto.UnitOfMeasure) ? "Unit" : dto.UnitOfMeasure.Trim();
        product.ReorderLevel = dto.ReorderLevel;
        product.Status = Enum.TryParse<RecordStatus>(dto.Status, true, out var st) ? st : RecordStatus.Active;

        _productRepository.Update(product);

        // Update inventory record with new product name and sku
        var inv = _inventoryRepository.GetByProductId(id);
        if (inv != null)
        {
            inv.ProductName = product.Name;
            inv.SKU = product.SKU;
            _inventoryRepository.Update(inv);
        }

        return new ProductDto
        {
            ProductId = product.ProductId,
            SKU = product.SKU,
            Name = product.Name,
            Category = product.Category,
            Description = product.Description,
            UnitPrice = product.UnitPrice,
            CostPrice = product.CostPrice,
            UnitOfMeasure = product.UnitOfMeasure,
            ReorderLevel = product.ReorderLevel,
            Status = product.Status.ToString(),
            QuantityOnHand = inv?.QuantityOnHand ?? 0,
            AvailableQuantity = inv?.AvailableQuantity ?? 0
        };
    }

    public bool Delete(int id)
    {
        var product = _productRepository.GetById(id);
        if (product == null) return false;

        _productRepository.Delete(id);
        return true;
    }
}

