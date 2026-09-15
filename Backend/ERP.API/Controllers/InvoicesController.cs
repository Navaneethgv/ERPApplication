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
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.Invoices.View)]
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

        var list = _invoiceService.GetAll(customerId);
        return Ok(list);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPolicies.Invoices.View)]
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

        var invoice = _invoiceService.GetById(id, customerId);
        if (invoice == null) return NotFound(new { message = $"Invoice with ID {id} not found." });
        return Ok(invoice);
    }

    [HttpPost("{id}/payments")]
    [Authorize(Policy = AppPolicies.Invoices.Edit)]
    public IActionResult RecordPayment(int id, [FromBody] RecordPaymentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var role = User.FindFirstValue(ClaimTypes.Role);
        if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
        {
            var custIdStr = User.FindFirstValue("CustomerId");
            if (!int.TryParse(custIdStr, out int cId))
            {
                return Forbid();
            }

            var invoice = _invoiceService.GetById(id, cId);
            if (invoice == null)
            {
                return NotFound(new { message = $"Invoice with ID {id} not found." });
            }
        }

        try
        {
            var user = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            var updated = _invoiceService.RecordPayment(id, dto, user);
            return Ok(updated);
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
}
