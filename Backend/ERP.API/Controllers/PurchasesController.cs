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
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;

    public PurchasesController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.Purchases.View)]
    public IActionResult GetAll()
    {
        var list = _purchaseService.GetAll();
        return Ok(list);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPolicies.Purchases.View)]
    public IActionResult GetById(int id)
    {
        var purchase = _purchaseService.GetById(id);
        if (purchase == null) return NotFound(new { message = $"Purchase order with ID {id} not found." });
        return Ok(purchase);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.Purchases.Add)]
    public IActionResult Create([FromBody] CreatePurchaseDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var user = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            var created = _purchaseService.Create(dto, user);
            return CreatedAtAction(nameof(GetById), new { id = created.PurchaseId }, created);
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
    [Authorize(Policy = AppPolicies.Purchases.Edit)]
    public IActionResult UpdateStatus(int id, [FromBody] UpdatePurchaseStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var user = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            var updated = _purchaseService.UpdateStatus(id, dto, user);
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
    [Authorize(Policy = AppPolicies.Purchases.Delete)]
    public IActionResult Delete(int id)
    {
        bool success = _purchaseService.Delete(id);
        if (!success) return NotFound(new { message = $"Purchase order with ID {id} not found." });
        return Ok(new { message = "Purchase order deleted successfully." });
    }
}
