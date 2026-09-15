namespace ERP.Application.DTOs;

public class ReportFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Category { get; set; }
    public int? CustomerId { get; set; }
    public int? SupplierId { get; set; }
    public string? Status { get; set; }
}

public class SalesReportSummaryDto
{
    public int TotalOrders { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int FulfilledOrders { get; set; }
    public int PendingOrders { get; set; }
    public List<SaleDto> Orders { get; set; } = new();
    public ChartDataDto CategorySalesChart { get; set; } = new();
    public ChartDataDto MonthlySalesTrendChart { get; set; } = new();
}

public class PurchaseReportSummaryDto
{
    public int TotalPurchases { get; set; }
    public decimal TotalPurchasedAmount { get; set; }
    public int ReceivedPurchases { get; set; }
    public int PendingPurchases { get; set; }
    public List<PurchaseDto> Purchases { get; set; } = new();
    public ChartDataDto SupplierSpendChart { get; set; } = new();
}

public class InventoryValuationReportDto
{
    public int TotalItems { get; set; }
    public int TotalUnitsOnHand { get; set; }
    public decimal TotalCostValuation { get; set; }
    public decimal TotalRetailValuation { get; set; }
    public decimal PotentialProfitMargin { get; set; }
    public int LowStockItemsCount { get; set; }
    public List<InventoryDto> InventoryItems { get; set; } = new();
}

public class FinancialSummaryReportDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit => TotalIncome - TotalExpenses;
    public decimal OutstandingReceivables { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public List<TransactionDto> Transactions { get; set; } = new();
    public ChartDataDto IncomeVsExpenseChart { get; set; } = new();
}

