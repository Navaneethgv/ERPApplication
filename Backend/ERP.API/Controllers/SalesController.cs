using System.Security.Claims;
using ERP.Application.DTOs;
using ERP.Application.Interfaces.Services;
using ERP.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly ISalesService _salesService;

    public SalesController(ISalesService salesService)
    {
        _salesService = salesService;
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.Sales.View)]
    public IActionResult GetAll()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        int? customerId = null;

        if (role == "Customer")
        {
            var custIdStr = User.FindFirstValue("CustomerId");
            if (!int.TryParse(custIdStr, out int cId))
            {
                return Forbid();
            }
            customerId = cId;
        }

        var list = _salesService.GetAll(customerId);
        return Ok(list);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPolicies.Sales.View)]
    public IActionResult GetById(int id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        int? customerId = null;

        if (role == "Customer")
        {
            var custIdStr = User.FindFirstValue("CustomerId");
            if (!int.TryParse(custIdStr, out int cId))
            {
                return Forbid();
            }
            customerId = cId;
        }

        var sale = _salesService.GetById(id, customerId);
        if (sale == null) return NotFound(new { message = $"Sales order with ID {id} not found." });
        return Ok(sale);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.Sales.Add)]
    public IActionResult Create([FromBody] CreateSaleDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var role = User.FindFirstValue(ClaimTypes.Role);
        int? customerIdFromToken = null;

        if (role == "Customer")
        {
            var custIdStr = User.FindFirstValue("CustomerId");
            if (!int.TryParse(custIdStr, out int cId))
            {
                return Forbid();
            }
            customerIdFromToken = cId;
        }

        try
        {
            var user = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            var created = _salesService.Create(dto, customerIdFromToken, user);
            return CreatedAtAction(nameof(GetById), new { id = created.SaleId }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Policy = AppPolicies.Sales.Edit)]
    public IActionResult UpdateStatus(int id, [FromBody] UpdateSaleStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var user = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            var updated = _salesService.UpdateStatus(id, dto, user);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AppPolicies.Sales.Delete)]
    public IActionResult Delete(int id)
    {
        bool success = _salesService.Delete(id);
        if (!success) return NotFound(new { message = $"Sales order with ID {id} not found." });
        return Ok(new { message = "Sales order deleted successfully." });
    }
}
