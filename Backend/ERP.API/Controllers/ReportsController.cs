using ERP.Application.DTOs;
using ERP.Application.Interfaces.Services;
using ERP.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("sales")]
    [Authorize(Policy = AppPolicies.Reports.View)]
    public IActionResult GetSalesReport([FromQuery] ReportFilterDto filter)
    {
        var report = _reportService.GetSalesReport(filter);
        return Ok(report);
    }

    [HttpGet("purchases")]
    [Authorize(Policy = AppPolicies.Reports.View)]
    public IActionResult GetPurchaseReport([FromQuery] ReportFilterDto filter)
    {
        var report = _reportService.GetPurchaseReport(filter);
        return Ok(report);
    }

    [HttpGet("inventory-valuation")]
    [Authorize(Policy = AppPolicies.Reports.View)]
    public IActionResult GetInventoryValuationReport([FromQuery] ReportFilterDto filter)
    {
        var report = _reportService.GetInventoryValuationReport(filter);
        return Ok(report);
    }

    [HttpGet("financial-summary")]
    [Authorize(Policy = AppPolicies.Reports.View)]
    public IActionResult GetFinancialSummaryReport([FromQuery] ReportFilterDto filter)
    {
        var report = _reportService.GetFinancialSummaryReport(filter);
        return Ok(report);
    }
}
