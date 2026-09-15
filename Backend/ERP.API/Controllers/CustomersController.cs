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
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.Customers.View)]
    public IActionResult GetAll()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Customer")
        {
            var custIdStr = User.FindFirstValue("CustomerId");
            if (int.TryParse(custIdStr, out int custId))
            {
                var single = _customerService.GetById(custId);
                return Ok(single != null ? new List<CustomerDto> { single } : new List<CustomerDto>());
            }
            return Forbid();
        }

        var list = _customerService.GetAll();
        return Ok(list);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPolicies.Customers.View)]
    public IActionResult GetById(int id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Customer")
        {
            var custIdStr = User.FindFirstValue("CustomerId");
            if (!int.TryParse(custIdStr, out int custId) || custId != id)
            {
                return Forbid();
            }
        }

        var customer = _customerService.GetById(id);
        if (customer == null) return NotFound(new { message = $"Customer with ID {id} not found." });
        return Ok(customer);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.Customers.Add)]
    public IActionResult Create([FromBody] CreateCustomerDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var created = _customerService.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.CustomerId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return BadRequest(new { message = "A customer with the specified email or details already exists." });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AppPolicies.Customers.Edit)]
    public IActionResult Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Customer")
        {
            var custIdStr = User.FindFirstValue("CustomerId");
            if (!int.TryParse(custIdStr, out int custId) || custId != id)
            {
                return Forbid();
            }
        }

        try
        {
            var updated = _customerService.Update(id, dto);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Customer with ID {id} not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return BadRequest(new { message = "A database constraint violation occurred while updating the customer." });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AppPolicies.Customers.Delete)]
    public IActionResult Delete(int id)
    {
        bool success = _customerService.Delete(id);
        if (!success) return NotFound(new { message = $"Customer with ID {id} not found." });
        return Ok(new { message = "Customer deleted successfully." });
    }
}
