



namespace ERP.Application.Services;
public class SalesService : ISalesService
{
    private readonly ISalesRepository _salesRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly INotificationService _notificationService;

    public SalesService(
        ISalesRepository salesRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IInventoryRepository inventoryRepository,
        IInvoiceRepository invoiceRepository,
        ITransactionRepository transactionRepository,
        INotificationService notificationService)
    {
        _salesRepository = salesRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _invoiceRepository = invoiceRepository;
        _transactionRepository = transactionRepository;
        _notificationService = notificationService;
    }

    public List<SaleDto> GetAll(int? customerId = null)
    {
        var sales = customerId.HasValue
            ? _salesRepository.GetByCustomerId(customerId.Value)
            : _salesRepository.GetAll();

        return sales.Select(MapToDto).ToList();
    }

    public SaleDto? GetById(int id, int? customerId = null)
    {
        var sale = _salesRepository.GetById(id);
        if (sale == null) return null;

        if (customerId.HasValue && sale.CustomerId != customerId.Value)
        {
            return null; // Forbidden customer access
        }

        return MapToDto(sale);
    }

    public SaleDto Create(CreateSaleDto dto, int? customerIdFromToken, string createdBy)
    {
        int targetCustomerId = customerIdFromToken ?? dto.CustomerId ?? 0;
        if (targetCustomerId <= 0)
        {
            throw new InvalidOperationException("Valid customer selection is required.");
        }

        var customer = _customerRepository.GetById(targetCustomerId);
        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with ID {targetCustomerId} not found.");
        }

        if (dto.Items == null || !dto.Items.Any())
        {
            throw new InvalidOperationException("Sales order must contain at least one item.");
        }

        // Optimized: Fetch only the products and inventories required for this order
        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = _productRepository.GetByIds(productIds).ToDictionary(p => p.ProductId);
        var inventories = _inventoryRepository.GetByProductIds(productIds).ToDictionary(i => i.ProductId);

        // 1. Stock Availability Check
        foreach (var itemDto in dto.Items)
        {
            if (!products.TryGetValue(itemDto.ProductId, out var prod))
            {
                throw new KeyNotFoundException($"Product with ID {itemDto.ProductId} not found.");
            }

            int availableStock = 0;
            if (inventories.TryGetValue(prod.ProductId, out var inv))
            {
                availableStock = inv.AvailableQuantity;
            }

            if (itemDto.Quantity > availableStock)
            {
                throw new InvalidOperationException($"Insufficient stock for '{prod.Name}' ({prod.SKU}). Requested: {itemDto.Quantity}, Available: {availableStock}.");
            }
        }

        // 2. Build Sale entity
        var sale = new Sale
        {
            CustomerId = customer.CustomerId,
            CustomerName = customer.Name,
            OrderDate = dto.OrderDate,
            DeliveryDate = dto.DeliveryDate,
            Status = SaleStatus.Confirmed, // Confirmed upon valid stock check
            Notes = dto.Notes?.Trim() ?? string.Empty,
            CreatedBy = createdBy,
            CreatedAt = TimeHelper.Now,
            Items = new List<SalesItem>()
        };

        decimal totalAmount = 0m;
        foreach (var itemDto in dto.Items)
        {
            var prod = products[itemDto.ProductId];
            decimal unitPrice = itemDto.UnitPrice > 0 ? itemDto.UnitPrice : prod.UnitPrice;

            var item = new SalesItem
            {
                ProductId = prod.ProductId,
                ProductName = prod.Name,
                SKU = prod.SKU,
                Quantity = itemDto.Quantity,
                UnitPrice = unitPrice
            };
            sale.Items.Add(item);
            totalAmount += item.TotalPrice;
        }

        sale.TotalAmount = totalAmount;
        sale = _salesRepository.Add(sale);

        // 3. Immediately fulfill / reserve stock, create invoice
        FulfillSaleInternal(sale, customer);

        // 4. Notify staff about the new order in real-time
        try
        {
            _notificationService.CreateOrderNotification(sale, customer.Name);
        }
        catch
        {
            // Logging or graceful fallback if notification fails
        }

        return MapToDto(sale);
    }

    public SaleDto UpdateStatus(int id, UpdateSaleStatusDto dto, string user)
    {
        var sale = _salesRepository.GetById(id);
        if (sale == null)
        {
            throw new KeyNotFoundException($"Sales order with ID {id} not found.");
        }

        var oldStatus = sale.Status;
        if (oldStatus == dto.Status)
        {
            return MapToDto(sale);
        }

        sale.Status = dto.Status;
        if (!string.IsNullOrEmpty(dto.Notes))
        {
            sale.Notes = string.IsNullOrEmpty(sale.Notes) ? dto.Notes : $"{sale.Notes}; {dto.Notes}";
        }

        var customer = _customerRepository.GetById(sale.CustomerId);

        if (dto.Status == SaleStatus.Fulfilled && oldStatus != SaleStatus.Fulfilled && customer != null)
        {
            FulfillSaleInternal(sale, customer);
        }

        _salesRepository.Update(sale);
        return MapToDto(sale);
    }

    private void FulfillSaleInternal(Sale sale, Customer customer)
    {
        // 1. Decrease Inventory
        foreach (var item in sale.Items)
        {
            _inventoryRepository.AdjustStock(item.ProductId, -item.Quantity, "Main Warehouse");
        }

        // 2. Generate Invoice if not exists
        var existingInv = _invoiceRepository.GetBySaleId(sale.SaleId);
        if (existingInv == null)
        {
            var invoice = new Invoice
            {
                SaleId = sale.SaleId,
                SaleOrderNumber = sale.SaleOrderNumber,
                CustomerId = customer.CustomerId,
                CustomerName = customer.Name,
                IssueDate = sale.OrderDate,
                DueDate = sale.OrderDate.AddDays(30),
                TotalAmount = sale.TotalAmount,
                PaidAmount = 0m,
                Status = InvoiceStatus.Unpaid,
                Notes = $"Invoice generated automatically from Order #{sale.SaleOrderNumber}",
                CreatedAt = TimeHelper.Now
            };
            _invoiceRepository.Add(invoice);

            // Update customer outstanding balance
            customer.CurrentBalance += sale.TotalAmount;
            _customerRepository.Update(customer);
        }
    }

    public bool Delete(int id)
    {
        var sale = _salesRepository.GetById(id);
        if (sale == null) return false;

        _salesRepository.Delete(id);
        return true;
    }

    private static SaleDto MapToDto(Sale s) => new()
    {
        SaleId = s.SaleId,
        SaleOrderNumber = s.SaleOrderNumber,
        CustomerId = s.CustomerId,
        CustomerName = s.CustomerName,
        OrderDate = s.OrderDate,
        DeliveryDate = s.DeliveryDate,
        TotalAmount = s.TotalAmount,
        Status = s.Status.ToString(),
        Notes = s.Notes,
        CreatedBy = s.CreatedBy,
        CreatedAt = s.CreatedAt,
        Items = s.Items.Select(i => new SalesItemDto
        {
            SalesItemId = i.SalesItemId,
            SaleId = i.SaleId,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            SKU = i.SKU,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList()
    };
}


