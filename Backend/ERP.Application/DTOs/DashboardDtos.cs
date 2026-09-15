using ERP.Domain.Entities;
using ERP.Domain.Enums;

namespace ERP.Application.DTOs;

public class ChartSeriesDto
{
    public string Name { get; set; } = string.Empty;
    public List<decimal> Data { get; set; } = new();
}

public class ChartDataDto
{
    public List<string> Labels { get; set; } = new();
    public List<ChartSeriesDto> Series { get; set; } = new();
    public List<decimal>? Values { get; set; }
}

public class AdminDashboardDto
{
    public int TotalEmployees { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public decimal TotalPurchasesAmount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal OutstandingReceivables { get; set; }

    public ChartDataDto SalesVsPurchasesChart { get; set; } = new();
    public ChartDataDto RevenueTrendsChart { get; set; } = new();
    public ChartDataDto CategoryDistributionChart { get; set; } = new();
    public ChartDataDto InventoryStatusChart { get; set; } = new();

    public List<TransactionDto> RecentTransactions { get; set; } = new();
    public List<SaleDto> RecentSales { get; set; } = new();
    public List<ProductDto> LowStockAlerts { get; set; } = new();
}

public class EmployeeDashboardDto
{
    public EmployeeDto? Profile { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public int TotalSalesOrders { get; set; }
    public int PendingOrders { get; set; }
    public int LowStockCount { get; set; }
    public decimal TotalSalesGenerated { get; set; }

    public ChartDataDto MonthlyPerformanceChart { get; set; } = new();
    public ChartDataDto OrdersStatusChart { get; set; } = new();

    public List<SaleDto> RecentSales { get; set; } = new();
    public List<PurchaseDto> RecentPurchases { get; set; } = new();
    public List<ProductDto> LowStockAlerts { get; set; } = new();
}

public class CustomerDashboardDto
{
    public CustomerDto? Profile { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public decimal TotalPurchasedAmount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public int UnpaidInvoicesCount { get; set; }

    public ChartDataDto OrderStatusChart { get; set; } = new();
    public ChartDataDto PurchaseHistoryChart { get; set; } = new();

    public List<SaleDto> RecentOrders { get; set; } = new();
    public List<InvoiceDto> RecentInvoices { get; set; } = new();
    public List<TransactionDto> RecentTransactions { get; set; } = new();
}

