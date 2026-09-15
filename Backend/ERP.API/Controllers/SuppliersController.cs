using ERP.Application.DTOs;
using ERP.Application.Interfaces.Services;
using ERP.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.Suppliers.View)]
    public IActionResult GetAll()
    {
        var list = _supplierService.GetAll();
        return Ok(list);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPolicies.Suppliers.View)]
    public IActionResult GetById(int id)
    {
        var s = _supplierService.GetById(id);
        if (s == null) return NotFound(new { message = $"Supplier with ID {id} not found." });
        return Ok(s);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.Suppliers.Add)]
    public IActionResult Create([FromBody] CreateSupplierDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var created = _supplierService.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.SupplierId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return BadRequest(new { message = "A supplier with the specified code or details already exists." });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AppPolicies.Suppliers.Edit)]
    public IActionResult Update(int id, [FromBody] UpdateSupplierDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var updated = _supplierService.Update(id, dto);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Supplier with ID {id} not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return BadRequest(new { message = "A database constraint violation occurred while updating the supplier." });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AppPolicies.Suppliers.Delete)]
    public IActionResult Delete(int id)
    {
        bool success = _supplierService.Delete(id);
        if (!success) return NotFound(new { message = $"Supplier with ID {id} not found." });
        return Ok(new { message = "Supplier deleted successfully." });
    }
}
