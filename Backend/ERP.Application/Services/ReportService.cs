



namespace ERP.Application.Services;
public class ReportService : IReportService
{
    private readonly ISalesRepository _salesRepository;
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public ReportService(
        ISalesRepository salesRepository,
        IPurchaseRepository purchaseRepository,
        IProductRepository productRepository,
        IInventoryRepository inventoryRepository,
        ITransactionRepository transactionRepository,
        IInvoiceRepository invoiceRepository)
    {
        _salesRepository = salesRepository;
        _purchaseRepository = purchaseRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _transactionRepository = transactionRepository;
        _invoiceRepository = invoiceRepository;
    }

    public SalesReportSummaryDto GetSalesReport(ReportFilterDto filter)
    {
        var sales = _salesRepository.GetAll();

        if (filter.StartDate.HasValue)
            sales = sales.Where(s => s.OrderDate >= filter.StartDate.Value.Date).ToList();
        if (filter.EndDate.HasValue)
            sales = sales.Where(s => s.OrderDate <= filter.EndDate.Value.Date.AddDays(1).AddTicks(-1)).ToList();
        if (filter.CustomerId.HasValue)
            sales = sales.Where(s => s.CustomerId == filter.CustomerId.Value).ToList();
        if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<SaleStatus>(filter.Status, true, out var st))
            sales = sales.Where(s => s.Status == st).ToList();

        var products = _productRepository.GetAll().ToDictionary(p => p.ProductId);
        var categorySales = new Dictionary<string, decimal>();

        foreach (var s in sales)
        {
            foreach (var item in s.Items)
            {
                string cat = products.TryGetValue(item.ProductId, out var p) ? p.Category : "General";
                categorySales[cat] = categorySales.GetValueOrDefault(cat) + item.TotalPrice;
            }
        }

        var monthlyTrends = sales
            .GroupBy(s => new { s.OrderDate.Year, s.OrderDate.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new
            {
                Month = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                Total = g.Sum(x => x.TotalAmount)
            }).ToList();

        decimal totalAmount = sales.Sum(s => s.TotalAmount);
        int totalOrders = sales.Count;

        return new SalesReportSummaryDto
        {
            TotalOrders = totalOrders,
            TotalSalesAmount = totalAmount,
            AverageOrderValue = totalOrders > 0 ? Math.Round(totalAmount / totalOrders, 2) : 0m,
            FulfilledOrders = sales.Count(s => s.Status == SaleStatus.Fulfilled),
            PendingOrders = sales.Count(s => s.Status == SaleStatus.Confirmed || s.Status == SaleStatus.Draft),
            Orders = sales.Select(s => new SaleDto
            {
                SaleId = s.SaleId,
                SaleOrderNumber = s.SaleOrderNumber,
                CustomerId = s.CustomerId,
                CustomerName = s.CustomerName,
                OrderDate = s.OrderDate,
                DeliveryDate = s.DeliveryDate,
                TotalAmount = s.TotalAmount,
                Status = s.Status.ToString(),
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
            }).ToList(),
            CategorySalesChart = new ChartDataDto
            {
                Labels = categorySales.Keys.ToList(),
                Values = categorySales.Values.ToList()
            },
            MonthlySalesTrendChart = new ChartDataDto
            {
                Labels = monthlyTrends.Select(m => m.Month).ToList(),
                Series = new List<ChartSeriesDto>
                {
                    new() { Name = "Sales Amount", Data = monthlyTrends.Select(m => m.Total).ToList() }
                }
            }
        };
    }

    public PurchaseReportSummaryDto GetPurchaseReport(ReportFilterDto filter)
    {
        var purchases = _purchaseRepository.GetAll();

        if (filter.StartDate.HasValue)
            purchases = purchases.Where(p => p.PurchaseDate >= filter.StartDate.Value.Date).ToList();
        if (filter.EndDate.HasValue)
            purchases = purchases.Where(p => p.PurchaseDate <= filter.EndDate.Value.Date.AddDays(1).AddTicks(-1)).ToList();
        if (filter.SupplierId.HasValue)
            purchases = purchases.Where(p => p.SupplierId == filter.SupplierId.Value).ToList();
        if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<PurchaseStatus>(filter.Status, true, out var st))
            purchases = purchases.Where(p => p.Status == st).ToList();

        var supplierSpend = purchases
            .GroupBy(p => p.SupplierName)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));

        return new PurchaseReportSummaryDto
        {
            TotalPurchases = purchases.Count,
            TotalPurchasedAmount = purchases.Sum(p => p.TotalAmount),
            ReceivedPurchases = purchases.Count(p => p.Status == PurchaseStatus.Received),
            PendingPurchases = purchases.Count(p => p.Status == PurchaseStatus.Ordered || p.Status == PurchaseStatus.Draft),
            Purchases = purchases.Select(p => new PurchaseDto
            {
                PurchaseId = p.PurchaseId,
                PurchaseNumber = p.PurchaseNumber,
                SupplierId = p.SupplierId,
                SupplierName = p.SupplierName,
                PurchaseDate = p.PurchaseDate,
                ExpectedDeliveryDate = p.ExpectedDeliveryDate,
                TotalAmount = p.TotalAmount,
                Status = p.Status.ToString()
            }).ToList(),
            SupplierSpendChart = new ChartDataDto
            {
                Labels = supplierSpend.Keys.ToList(),
                Values = supplierSpend.Values.ToList()
            }
        };
    }

    public InventoryValuationReportDto GetInventoryValuationReport(ReportFilterDto filter)
    {
        var inventory = _inventoryRepository.GetAll();
        var products = _productRepository.GetAll().ToDictionary(p => p.ProductId);

        var list = new List<InventoryDto>();
        decimal totalCost = 0m;
        decimal totalRetail = 0m;
        int totalUnits = 0;
        int lowStockCount = 0;

        foreach (var i in inventory)
        {
            products.TryGetValue(i.ProductId, out var prod);
            if (filter.Category != null && prod?.Category != filter.Category)
                continue;

            decimal unitPrice = prod?.UnitPrice ?? 0m;
            decimal costPrice = prod?.CostPrice ?? 0m;
            int reorder = prod?.ReorderLevel ?? 10;
            bool isLow = i.QuantityOnHand <= reorder;
            if (isLow) lowStockCount++;

            totalCost += i.QuantityOnHand * costPrice;
            totalRetail += i.QuantityOnHand * unitPrice;
            totalUnits += i.QuantityOnHand;

            list.Add(new InventoryDto
            {
                InventoryId = i.InventoryId,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                SKU = i.SKU,
                Category = prod?.Category ?? "General",
                QuantityOnHand = i.QuantityOnHand,
                ReservedQuantity = i.ReservedQuantity,
                UnitPrice = unitPrice,
                CostPrice = costPrice,
                ReorderLevel = reorder,
                Location = i.Location,
                LastUpdated = i.LastUpdated
            });
        }

        decimal potentialMargin = totalRetail > 0 ? Math.Round(((totalRetail - totalCost) / totalRetail) * 100m, 2) : 0m;

        return new InventoryValuationReportDto
        {
            TotalItems = list.Count,
            TotalUnitsOnHand = totalUnits,
            TotalCostValuation = totalCost,
            TotalRetailValuation = totalRetail,
            PotentialProfitMargin = potentialMargin,
            LowStockItemsCount = lowStockCount,
            InventoryItems = list
        };
    }

    public FinancialSummaryReportDto GetFinancialSummaryReport(ReportFilterDto filter)
    {
        var transactions = _transactionRepository.GetAll();

        if (filter.StartDate.HasValue)
            transactions = transactions.Where(t => t.TransactionDate >= filter.StartDate.Value.Date).ToList();
        if (filter.EndDate.HasValue)
            transactions = transactions.Where(t => t.TransactionDate <= filter.EndDate.Value.Date.AddDays(1).AddTicks(-1)).ToList();

        decimal totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        decimal totalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        var invoices = _invoiceRepository.GetAll();
        decimal outstandingReceivables = invoices.Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled).Sum(i => i.BalanceAmount);

        var invVal = GetInventoryValuationReport(new ReportFilterDto());

        return new FinancialSummaryReportDto
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            OutstandingReceivables = outstandingReceivables,
            TotalInventoryValue = invVal.TotalCostValuation,
            Transactions = transactions.Select(t => new TransactionDto
            {
                TransactionId = t.TransactionId,
                TransactionNumber = t.TransactionNumber,
                ReferenceType = t.ReferenceType.ToString(),
                ReferenceNumber = t.ReferenceNumber,
                Type = t.Type.ToString(),
                Category = t.Category,
                Amount = t.Amount,
                TransactionDate = t.TransactionDate,
                PaymentMethod = t.PaymentMethod,
                Status = t.Status,
                Notes = t.Notes,
                CreatedBy = t.CreatedBy
            }).ToList(),
            IncomeVsExpenseChart = new ChartDataDto
            {
                Labels = new List<string> { "Total Income", "Total Expenses", "Net Margin" },
                Values = new List<decimal> { totalIncome, totalExpenses, Math.Max(0, totalIncome - totalExpenses) }
            }
        };
    }
}

