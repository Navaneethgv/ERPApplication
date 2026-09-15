using ERP.Application.DTOs;
using ERP.Application.Interfaces.Services;
using ERP.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.Products.View)]
    public IActionResult GetAll()
    {
        var list = _productService.GetAll();
        return Ok(list);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPolicies.Products.View)]
    public IActionResult GetById(int id)
    {
        var p = _productService.GetById(id);
        if (p == null) return NotFound(new { message = $"Product with ID {id} not found." });
        return Ok(p);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.Products.Add)]
    public IActionResult Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var created = _productService.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.ProductId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return BadRequest(new { message = "A product with the specified SKU or identifier already exists." });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AppPolicies.Products.Edit)]
    public IActionResult Update(int id, [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var updated = _productService.Update(id, dto);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Product with ID {id} not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return BadRequest(new { message = "A database constraint violation occurred while updating the product." });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AppPolicies.Products.Delete)]
    public IActionResult Delete(int id)
    {
        bool success = _productService.Delete(id);
        if (!success) return NotFound(new { message = $"Product with ID {id} not found." });
        return Ok(new { message = "Product deleted successfully." });
    }
}
