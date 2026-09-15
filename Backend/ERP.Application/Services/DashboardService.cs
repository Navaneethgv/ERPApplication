


using System.Globalization;

namespace ERP.Application.Services;
public class DashboardService : IDashboardService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly ISalesRepository _salesRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ITransactionRepository _transactionRepository;

    public DashboardService(
        IEmployeeRepository employeeRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IInventoryRepository inventoryRepository,
        IPurchaseRepository purchaseRepository,
        ISalesRepository salesRepository,
        IInvoiceRepository invoiceRepository,
        ITransactionRepository transactionRepository)
    {
        _employeeRepository = employeeRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _purchaseRepository = purchaseRepository;
        _salesRepository = salesRepository;
        _invoiceRepository = invoiceRepository;
        _transactionRepository = transactionRepository;
    }

    public AdminDashboardDto GetAdminDashboard()
    {
        var employees = _employeeRepository.GetAll();
        var customers = _customerRepository.GetAll();
        var products = _productRepository.GetAll();
        var inventory = _inventoryRepository.GetAll().ToDictionary(i => i.ProductId);
        var purchases = _purchaseRepository.GetAll();
        var sales = _salesRepository.GetAll();
        var invoices = _invoiceRepository.GetAll();
        var transactions = _transactionRepository.GetAll();

        var lowStockProducts = products.Where(p =>
        {
            var inv = inventory.TryGetValue(p.ProductId, out var item) ? item : null;
            return (inv?.QuantityOnHand ?? 0) <= p.ReorderLevel;
        }).ToList();

        // Months for trends (last 6 months)
        var now = TimeHelper.Now;
        var monthLabels = new List<string>();
        var salesMonthly = new List<decimal>();
        var purchasesMonthly = new List<decimal>();
        var incomeMonthly = new List<decimal>();

        for (int i = 5; i >= 0; i--)
        {
            var mDate = now.AddMonths(-i);
            string mLabel = mDate.ToString("MMM yyyy");
            monthLabels.Add(mLabel);

            var sTotal = sales
                .Where(s => s.OrderDate.Year == mDate.Year && s.OrderDate.Month == mDate.Month && s.Status != SaleStatus.Cancelled)
                .Sum(s => s.TotalAmount);
            salesMonthly.Add(sTotal);

            var pTotal = purchases
                .Where(p => p.PurchaseDate.Year == mDate.Year && p.PurchaseDate.Month == mDate.Month && p.Status != PurchaseStatus.Cancelled)
                .Sum(p => p.TotalAmount);
            purchasesMonthly.Add(pTotal);

            var incTotal = transactions
                .Where(t => t.TransactionDate.Year == mDate.Year && t.TransactionDate.Month == mDate.Month && t.Type == TransactionType.Income)
                .Sum(t => t.Amount);
            incomeMonthly.Add(incTotal);
        }

        // Category breakdown
        var categories = products.GroupBy(p => p.Category)
            .Select(g => new { Category = g.Key, Count = (decimal)g.Count() })
            .ToList();

        // Inventory status
        int inStockCount = products.Count(p => (inventory.TryGetValue(p.ProductId, out var inv) ? inv.QuantityOnHand : 0) > p.ReorderLevel);
        int lowStockCount = lowStockProducts.Count(p => (inventory.TryGetValue(p.ProductId, out var inv) ? inv.QuantityOnHand : 0) > 0);
        int outOfStockCount = products.Count(p => (inventory.TryGetValue(p.ProductId, out var inv) ? inv.QuantityOnHand : 0) <= 0);

        return new AdminDashboardDto
        {
            TotalEmployees = employees.Count(e => e.Status == RecordStatus.Active),
            TotalCustomers = customers.Count(c => c.Status == RecordStatus.Active),
            TotalProducts = products.Count(p => p.Status == RecordStatus.Active),
            LowStockCount = lowStockProducts.Count,
            TotalSalesAmount = sales.Where(s => s.Status != SaleStatus.Cancelled).Sum(s => s.TotalAmount),
            TotalPurchasesAmount = purchases.Where(p => p.Status != PurchaseStatus.Cancelled).Sum(p => p.TotalAmount),
            TotalRevenue = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
            OutstandingReceivables = invoices.Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled).Sum(i => i.BalanceAmount),

            SalesVsPurchasesChart = new ChartDataDto
            {
                Labels = monthLabels,
                Series = new List<ChartSeriesDto>
                {
                    new() { Name = "Sales", Data = salesMonthly },
                    new() { Name = "Purchases", Data = purchasesMonthly }
                }
            },
            RevenueTrendsChart = new ChartDataDto
            {
                Labels = monthLabels,
                Series = new List<ChartSeriesDto>
                {
                    new() { Name = "Revenue", Data = incomeMonthly }
                }
            },
            CategoryDistributionChart = new ChartDataDto
            {
                Labels = categories.Select(c => c.Category).ToList(),
                Values = categories.Select(c => c.Count).ToList()
            },
            InventoryStatusChart = new ChartDataDto
            {
                Labels = new List<string> { "In Stock", "Low Stock", "Out of Stock" },
                Values = new List<decimal> { inStockCount, lowStockCount, outOfStockCount }
            },
            RecentTransactions = transactions.Take(6).Select(t => new TransactionDto
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
            RecentSales = sales.Take(5).Select(s => new SaleDto
            {
                SaleId = s.SaleId,
                SaleOrderNumber = s.SaleOrderNumber,
                CustomerId = s.CustomerId,
                CustomerName = s.CustomerName,
                OrderDate = s.OrderDate,
                TotalAmount = s.TotalAmount,
                Status = s.Status.ToString()
            }).ToList(),
            LowStockAlerts = lowStockProducts.Take(5).Select(p =>
            {
                var inv = inventory.TryGetValue(p.ProductId, out var item) ? item : null;
                return new ProductDto
                {
                    ProductId = p.ProductId,
                    SKU = p.SKU,
                    Name = p.Name,
                    Category = p.Category,
                    UnitPrice = p.UnitPrice,
                    ReorderLevel = p.ReorderLevel,
                    QuantityOnHand = inv?.QuantityOnHand ?? 0,
                    AvailableQuantity = inv?.AvailableQuantity ?? 0
                };
            }).ToList()
        };
    }

    public EmployeeDashboardDto GetEmployeeDashboard(int? employeeId)
    {
        Employee? emp = null;
        if (employeeId.HasValue)
        {
            emp = _employeeRepository.GetById(employeeId.Value);
        }

        var sales = _salesRepository.GetAll();
        var purchases = _purchaseRepository.GetAll();
        var products = _productRepository.GetAll();
        var inventory = _inventoryRepository.GetAll().ToDictionary(i => i.ProductId);

        var lowStock = products.Where(p =>
        {
            var inv = inventory.TryGetValue(p.ProductId, out var item) ? item : null;
            return (inv?.QuantityOnHand ?? 0) <= p.ReorderLevel;
        }).ToList();

        var now = TimeHelper.Now;
        var monthLabels = new List<string>();
        var performanceData = new List<decimal>();

        for (int i = 5; i >= 0; i--)
        {
            var mDate = now.AddMonths(-i);
            monthLabels.Add(mDate.ToString("MMM yyyy"));

            var sTotal = sales
                .Where(s => s.OrderDate.Year == mDate.Year && s.OrderDate.Month == mDate.Month && s.Status != SaleStatus.Cancelled)
                .Sum(s => s.TotalAmount);
            performanceData.Add(sTotal);
        }

        int confirmedSales = sales.Count(s => s.Status == SaleStatus.Confirmed);
        int fulfilledSales = sales.Count(s => s.Status == SaleStatus.Fulfilled);
        int draftSales = sales.Count(s => s.Status == SaleStatus.Draft);

        return new EmployeeDashboardDto
        {
            Profile = emp == null ? null : new EmployeeDto
            {
                EmployeeId = emp.EmployeeId,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                Email = emp.Email,
                Department = emp.Department,
                Designation = emp.Designation,
                Salary = emp.Salary,
                Status = emp.Status.ToString()
            },
            Department = emp?.Department ?? "Operations",
            Designation = emp?.Designation ?? "Specialist",
            TotalSalesOrders = sales.Count,
            PendingOrders = sales.Count(s => s.Status == SaleStatus.Confirmed || s.Status == SaleStatus.Draft),
            LowStockCount = lowStock.Count,
            TotalSalesGenerated = sales.Where(s => s.Status != SaleStatus.Cancelled).Sum(s => s.TotalAmount),

            MonthlyPerformanceChart = new ChartDataDto
            {
                Labels = monthLabels,
                Series = new List<ChartSeriesDto>
                {
                    new() { Name = "Sales Volume ($)", Data = performanceData }
                }
            },
            OrdersStatusChart = new ChartDataDto
            {
                Labels = new List<string> { "Confirmed", "Fulfilled", "Draft" },
                Values = new List<decimal> { confirmedSales, fulfilledSales, draftSales }
            },
            RecentSales = sales.Take(5).Select(s => new SaleDto
            {
                SaleId = s.SaleId,
                SaleOrderNumber = s.SaleOrderNumber,
                CustomerId = s.CustomerId,
                CustomerName = s.CustomerName,
                OrderDate = s.OrderDate,
                TotalAmount = s.TotalAmount,
                Status = s.Status.ToString()
            }).ToList(),
            RecentPurchases = purchases.Take(5).Select(p => new PurchaseDto
            {
                PurchaseId = p.PurchaseId,
                PurchaseNumber = p.PurchaseNumber,
                SupplierId = p.SupplierId,
                SupplierName = p.SupplierName,
                PurchaseDate = p.PurchaseDate,
                TotalAmount = p.TotalAmount,
                Status = p.Status.ToString()
            }).ToList(),
            LowStockAlerts = lowStock.Take(5).Select(p =>
            {
                var inv = inventory.TryGetValue(p.ProductId, out var item) ? item : null;
                return new ProductDto
                {
                    ProductId = p.ProductId,
                    SKU = p.SKU,
                    Name = p.Name,
                    Category = p.Category,
                    UnitPrice = p.UnitPrice,
                    ReorderLevel = p.ReorderLevel,
                    QuantityOnHand = inv?.QuantityOnHand ?? 0,
                    AvailableQuantity = inv?.AvailableQuantity ?? 0
                };
            }).ToList()
        };
    }

    public CustomerDashboardDto GetCustomerDashboard(int customerId)
    {
        var cust = _customerRepository.GetById(customerId);
        var sales = _salesRepository.GetByCustomerId(customerId);
        var invoices = _invoiceRepository.GetByCustomerId(customerId);

        // Optimized: Fetch only the transactions corresponding to this customer's invoice numbers
        var invoiceNumbers = invoices.Select(i => i.InvoiceNumber).ToList();
        var transactions = _transactionRepository.GetByReferenceNumbers(invoiceNumbers);

        var now = TimeHelper.Now;
        var monthLabels = new List<string>();
        var spendData = new List<decimal>();

        for (int i = 5; i >= 0; i--)
        {
            var mDate = now.AddMonths(-i);
            monthLabels.Add(mDate.ToString("MMM yyyy"));

            var monthlySpend = sales
                .Where(s => s.OrderDate.Year == mDate.Year && s.OrderDate.Month == mDate.Month && s.Status != SaleStatus.Cancelled)
                .Sum(s => s.TotalAmount);
            spendData.Add(monthlySpend);
        }

        int confirmed = sales.Count(s => s.Status == SaleStatus.Confirmed);
        int fulfilled = sales.Count(s => s.Status == SaleStatus.Fulfilled);
        int draft = sales.Count(s => s.Status == SaleStatus.Draft);

        return new CustomerDashboardDto
        {
            Profile = cust == null ? null : new CustomerDto
            {
                CustomerId = cust.CustomerId,
                Name = cust.Name,
                ContactPerson = cust.ContactPerson,
                Email = cust.Email,
                Phone = cust.Phone,
                Company = cust.Company,
                CreditLimit = cust.CreditLimit,
                CurrentBalance = cust.CurrentBalance,
                Status = cust.Status.ToString(),
                CreatedAt = cust.CreatedAt
            },
            TotalOrders = sales.Count,
            PendingOrders = sales.Count(s => s.Status == SaleStatus.Confirmed || s.Status == SaleStatus.Draft),
            TotalPurchasedAmount = sales.Where(s => s.Status != SaleStatus.Cancelled).Sum(s => s.TotalAmount),
            OutstandingBalance = cust?.CurrentBalance ?? invoices.Where(i => i.Status != InvoiceStatus.Paid).Sum(i => i.BalanceAmount),
            UnpaidInvoicesCount = invoices.Count(i => i.Status == InvoiceStatus.Unpaid || i.Status == InvoiceStatus.PartiallyPaid),

            OrderStatusChart = new ChartDataDto
            {
                Labels = new List<string> { "Fulfilled", "Confirmed", "Draft" },
                Values = new List<decimal> { fulfilled, confirmed, draft }
            },
            PurchaseHistoryChart = new ChartDataDto
            {
                Labels = monthLabels,
                Series = new List<ChartSeriesDto>
                {
                    new() { Name = "Spend ($)", Data = spendData }
                }
            },
            RecentOrders = sales.Take(5).Select(s => new SaleDto
            {
                SaleId = s.SaleId,
                SaleOrderNumber = s.SaleOrderNumber,
                CustomerId = s.CustomerId,
                CustomerName = s.CustomerName,
                OrderDate = s.OrderDate,
                TotalAmount = s.TotalAmount,
                Status = s.Status.ToString()
            }).ToList(),
            RecentInvoices = invoices.Take(5).Select(i => new InvoiceDto
            {
                InvoiceId = i.InvoiceId,
                InvoiceNumber = i.InvoiceNumber,
                SaleId = i.SaleId,
                SaleOrderNumber = i.SaleOrderNumber,
                CustomerId = i.CustomerId,
                CustomerName = i.CustomerName,
                IssueDate = i.IssueDate,
                DueDate = i.DueDate,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                Status = i.Status.ToString()
            }).ToList(),
            RecentTransactions = transactions.Take(5).Select(t => new TransactionDto
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
                Status = t.Status
            }).ToList()
        };
    }
}


