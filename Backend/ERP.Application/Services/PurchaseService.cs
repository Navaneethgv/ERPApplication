



namespace ERP.Application.Services;
public class PurchaseService : IPurchaseService
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ITransactionRepository _transactionRepository;

    public PurchaseService(
        IPurchaseRepository purchaseRepository,
        ISupplierRepository supplierRepository,
        IProductRepository productRepository,
        IInventoryRepository inventoryRepository,
        ITransactionRepository transactionRepository)
    {
        _purchaseRepository = purchaseRepository;
        _supplierRepository = supplierRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _transactionRepository = transactionRepository;
    }

    public List<PurchaseDto> GetAll()
    {
        return _purchaseRepository.GetAll().Select(MapToDto).ToList();
    }

    public PurchaseDto? GetById(int id)
    {
        var p = _purchaseRepository.GetById(id);
        return p == null ? null : MapToDto(p);
    }

    public PurchaseDto Create(CreatePurchaseDto dto, string createdBy)
    {
        var supplier = _supplierRepository.GetById(dto.SupplierId);
        if (supplier == null)
        {
            throw new KeyNotFoundException($"Supplier with ID {dto.SupplierId} not found.");
        }

        if (dto.Items == null || !dto.Items.Any())
        {
            throw new InvalidOperationException("Purchase order must have at least one line item.");
        }

        // Optimized: Fetch only the specific products referenced in this purchase order
        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = _productRepository.GetByIds(productIds).ToDictionary(p => p.ProductId);

        var purchase = new Purchase
        {
            SupplierId = supplier.SupplierId,
            SupplierName = supplier.Name,
            PurchaseDate = dto.PurchaseDate,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
            Status = PurchaseStatus.Ordered, // By default newly created PO is 'Ordered'
            Notes = dto.Notes?.Trim() ?? string.Empty,
            CreatedBy = createdBy,
            CreatedAt = TimeHelper.Now,
            Items = new List<PurchaseItem>()
        };

        decimal totalAmount = 0m;
        foreach (var itemDto in dto.Items)
        {
            if (!products.TryGetValue(itemDto.ProductId, out var prod))
            {
                throw new KeyNotFoundException($"Product with ID {itemDto.ProductId} not found.");
            }

            decimal price = itemDto.UnitPrice > 0 ? itemDto.UnitPrice : prod.CostPrice;
            var item = new PurchaseItem
            {
                ProductId = prod.ProductId,
                ProductName = prod.Name,
                SKU = prod.SKU,
                Quantity = itemDto.Quantity,
                UnitPrice = price
            };
            purchase.Items.Add(item);
            totalAmount += item.TotalPrice;
        }

        purchase.TotalAmount = totalAmount;
        purchase = _purchaseRepository.Add(purchase);

        return MapToDto(purchase);
    }

    public PurchaseDto UpdateStatus(int id, UpdatePurchaseStatusDto dto, string user)
    {
        var purchase = _purchaseRepository.GetById(id);
        if (purchase == null)
        {
            throw new KeyNotFoundException($"Purchase order with ID {id} not found.");
        }

        var oldStatus = purchase.Status;
        if (oldStatus == dto.Status)
        {
            return MapToDto(purchase);
        }

        purchase.Status = dto.Status;
        if (!string.IsNullOrEmpty(dto.Notes))
        {
            purchase.Notes = string.IsNullOrEmpty(purchase.Notes) ? dto.Notes : $"{purchase.Notes}; {dto.Notes}";
        }

        // WORKFLOW: When Purchase is Received -> Increase Inventory & Record Expense Transaction
        if (dto.Status == PurchaseStatus.Received && oldStatus != PurchaseStatus.Received)
        {
            foreach (var item in purchase.Items)
            {
                _inventoryRepository.AdjustStock(item.ProductId, item.Quantity, "Main Warehouse");
            }

            // Create Expense Transaction
            _transactionRepository.Add(new Transaction
            {
                ReferenceType = ReferenceType.Purchase,
                ReferenceNumber = purchase.PurchaseNumber,
                Type = TransactionType.Expense,
                Category = "Inventory Purchase",
                Amount = purchase.TotalAmount,
                TransactionDate = TimeHelper.Now,
                PaymentMethod = "Bank Transfer",
                Status = "Completed",
                Notes = $"Goods received for PO #{purchase.PurchaseNumber} from {purchase.SupplierName}",
                CreatedBy = user
            });
        }

        _purchaseRepository.Update(purchase);
        return MapToDto(purchase);
    }

    public bool Delete(int id)
    {
        var purchase = _purchaseRepository.GetById(id);
        if (purchase == null) return false;

        _purchaseRepository.Delete(id);
        return true;
    }

    private static PurchaseDto MapToDto(Purchase p) => new()
    {
        PurchaseId = p.PurchaseId,
        PurchaseNumber = p.PurchaseNumber,
        SupplierId = p.SupplierId,
        SupplierName = p.SupplierName,
        PurchaseDate = p.PurchaseDate,
        ExpectedDeliveryDate = p.ExpectedDeliveryDate,
        TotalAmount = p.TotalAmount,
        Status = p.Status.ToString(),
        Notes = p.Notes,
        CreatedBy = p.CreatedBy,
        CreatedAt = p.CreatedAt,
        Items = p.Items.Select(i => new PurchaseItemDto
        {
            PurchaseItemId = i.PurchaseItemId,
            PurchaseId = i.PurchaseId,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            SKU = i.SKU,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList()
    };
}


