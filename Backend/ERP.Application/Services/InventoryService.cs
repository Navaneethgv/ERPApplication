



namespace ERP.Application.Services;
public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly ITransactionRepository _transactionRepository;

    public InventoryService(
        IInventoryRepository inventoryRepository,
        IProductRepository productRepository,
        ITransactionRepository transactionRepository)
    {
        _inventoryRepository = inventoryRepository;
        _productRepository = productRepository;
        _transactionRepository = transactionRepository;
    }

    public List<InventoryDto> GetAll()
    {
        var inventories = _inventoryRepository.GetAll();
        var products = _productRepository.GetAll().ToDictionary(p => p.ProductId);

        return inventories.Select(i =>
        {
            products.TryGetValue(i.ProductId, out var prod);
            return new InventoryDto
            {
                InventoryId = i.InventoryId,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                SKU = i.SKU,
                Category = prod?.Category ?? "General",
                QuantityOnHand = i.QuantityOnHand,
                ReservedQuantity = i.ReservedQuantity,
                UnitPrice = prod?.UnitPrice ?? 0m,
                CostPrice = prod?.CostPrice ?? 0m,
                ReorderLevel = prod?.ReorderLevel ?? 10,
                Location = i.Location,
                LastUpdated = i.LastUpdated
            };
        }).ToList();
    }

    public InventoryDto? GetByProductId(int productId)
    {
        var inv = _inventoryRepository.GetByProductId(productId);
        if (inv == null) return null;

        var prod = _productRepository.GetById(productId);
        return new InventoryDto
        {
            InventoryId = inv.InventoryId,
            ProductId = inv.ProductId,
            ProductName = inv.ProductName,
            SKU = inv.SKU,
            Category = prod?.Category ?? "General",
            QuantityOnHand = inv.QuantityOnHand,
            ReservedQuantity = inv.ReservedQuantity,
            UnitPrice = prod?.UnitPrice ?? 0m,
            CostPrice = prod?.CostPrice ?? 0m,
            ReorderLevel = prod?.ReorderLevel ?? 10,
            Location = inv.Location,
            LastUpdated = inv.LastUpdated
        };
    }

    public InventoryDto AdjustStock(StockAdjustmentDto dto, string createdBy)
    {
        var product = _productRepository.GetById(dto.ProductId);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {dto.ProductId} not found.");
        }

        int quantityDelta = dto.AdjustmentType switch
        {
            InventoryAdjustmentType.StockIn => Math.Abs(dto.Quantity),
            InventoryAdjustmentType.StockOut => -Math.Abs(dto.Quantity),
            InventoryAdjustmentType.Damaged => -Math.Abs(dto.Quantity),
            InventoryAdjustmentType.Correction => dto.Quantity,
            InventoryAdjustmentType.Audit => dto.Quantity,
            _ => dto.Quantity
        };

        _inventoryRepository.AdjustStock(dto.ProductId, quantityDelta, dto.Location);

        // Record a transaction audit log if financial impact or significant adjustment
        if (dto.AdjustmentType == InventoryAdjustmentType.Damaged || dto.AdjustmentType == InventoryAdjustmentType.StockOut)
        {
            _transactionRepository.Add(new Transaction
            {
                ReferenceType = ReferenceType.Adjustment,
                ReferenceNumber = $"ADJ-{product.SKU}-{TimeHelper.Now:yyyyMMdd}",
                Type = TransactionType.Expense,
                Category = "Inventory Loss / Write-off",
                Amount = Math.Abs(quantityDelta) * product.CostPrice,
                TransactionDate = TimeHelper.Now,
                PaymentMethod = "N/A",
                Status = "Completed",
                Notes = $"Stock Adjustment: {dto.AdjustmentType} for {product.Name} (Qty: {quantityDelta}). Reason: {dto.Reason}",
                CreatedBy = createdBy
            });
        }

        return GetByProductId(dto.ProductId)!;
    }

    public List<InventoryDto> GetLowStockItems()
    {
        return GetAll().Where(i => i.IsLowStock).ToList();
    }
}


