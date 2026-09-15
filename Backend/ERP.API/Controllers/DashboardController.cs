using System.Security.Claims;
using ERP.Application.Interfaces.Services;
using ERP.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    [Authorize(Policy = AppPolicies.Dashboard.View)]
    public IActionResult GetDashboardSummary()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            var adminData = _dashboardService.GetAdminDashboard();
            return Ok(new { role = "Admin", data = adminData });
        }
        else if (string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase))
        {
            var empIdStr = User.FindFirstValue("EmployeeId");
            int? empId = int.TryParse(empIdStr, out int id) ? id : null;
            var employeeData = _dashboardService.GetEmployeeDashboard(empId);
            return Ok(new { role = "Employee", data = employeeData });
        }
        else if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
        {
            var custIdStr = User.FindFirstValue("CustomerId");
            if (!int.TryParse(custIdStr, out int custId))
            {
                return BadRequest(new { message = "Customer record not found for this account." });
            }
            var customerData = _dashboardService.GetCustomerDashboard(custId);
            return Ok(new { role = "Customer", data = customerData });
        }

        return Forbid();
    }

    [HttpGet("admin")]
    [Authorize(Policy = AppPolicies.Security.View)]
    public IActionResult GetAdminDashboard()
    {
        var data = _dashboardService.GetAdminDashboard();
        return Ok(data);
    }

    [HttpGet("employee")]
    [Authorize(Policy = AppPolicies.Dashboard.View)]
    public IActionResult GetEmployeeDashboard()
    {
        var empIdStr = User.FindFirstValue("EmployeeId");
        int? empId = int.TryParse(empIdStr, out int id) ? id : null;
        var data = _dashboardService.GetEmployeeDashboard(empId);
        return Ok(data);
    }

    [HttpGet("customer")]
    [Authorize(Policy = AppPolicies.Dashboard.View)]
    public IActionResult GetCustomerDashboard([FromQuery] int? customerId = null)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        int targetCustomerId;

        if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
        {
            var custIdStr = User.FindFirstValue("CustomerId");
            if (!int.TryParse(custIdStr, out int custId))
            {
                return BadRequest(new { message = "Customer ID not associated with current account." });
            }
            targetCustomerId = custId;
        }
        else
        {
            // Admin role can specify a customerId or default to 1 (seeded active customer)
            if (customerId.HasValue && customerId.Value > 0)
            {
                targetCustomerId = customerId.Value;
            }
            else
            {
                var custIdStr = User.FindFirstValue("CustomerId");
                if (int.TryParse(custIdStr, out int custId))
                { 
                    targetCustomerId = custId; 
                }
                else
                {
                    targetCustomerId = 1;
                }
            }
        }

        var data = _dashboardService.GetCustomerDashboard(targetCustomerId);
        if (data == null)
        {
            return NotFound(new { message = $"Customer data for ID {targetCustomerId} not found." });
        }
        return Ok(data);
    }
}
