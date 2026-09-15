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
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.Inventory.View)]
    public IActionResult GetAll()
    {
        var list = _inventoryService.GetAll();
        return Ok(list);
    }

    [HttpGet("product/{productId}")]
    [Authorize(Policy = AppPolicies.Inventory.View)]
    public IActionResult GetByProductId(int productId)
    {
        var inv = _inventoryService.GetByProductId(productId);
        if (inv == null) return NotFound(new { message = $"Inventory record for product ID {productId} not found." });
        return Ok(inv);
    }

    [HttpGet("low-stock")]
    [Authorize(Policy = AppPolicies.Inventory.View)]
    public IActionResult GetLowStock()
    {
        var list = _inventoryService.GetLowStockItems();
        return Ok(list);
    }

    [HttpPost("adjust")]
    [Authorize(Policy = AppPolicies.Inventory.Edit)]
    public IActionResult AdjustStock([FromBody] StockAdjustmentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var user = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            var result = _inventoryService.AdjustStock(dto, user);
            return Ok(result);
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
}
