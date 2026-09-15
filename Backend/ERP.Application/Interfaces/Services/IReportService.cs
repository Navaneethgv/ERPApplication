using ERP.Application.DTOs;

namespace ERP.Application.Interfaces.Services;

public interface IReportService
{
    SalesReportSummaryDto GetSalesReport(ReportFilterDto filter);
    PurchaseReportSummaryDto GetPurchaseReport(ReportFilterDto filter);
    InventoryValuationReportDto GetInventoryValuationReport(ReportFilterDto filter);
    FinancialSummaryReportDto GetFinancialSummaryReport(ReportFilterDto filter);
}
