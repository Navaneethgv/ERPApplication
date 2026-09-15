using ERP.Application.DTOs;
using ERP.Application.Interfaces.Services;
using ERP.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet("get-all")]
    [Authorize(Policy = AppPolicies.Employees.View)]
    public IActionResult GetAll()
    {
        var list = _employeeService.GetAll();
        return Ok(list);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPolicies.Employees.View)]
    public IActionResult GetById(int id)
    {
        var emp = _employeeService.GetById(id);
        if (emp == null) return NotFound(new { message = $"Employee with ID {id} not found." });
        return Ok(emp);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.Employees.Add)]
    public IActionResult Create([FromBody] CreateEmployeeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var created = _employeeService.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.EmployeeId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return BadRequest(new { message = "An employee with the specified email or details already exists." });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AppPolicies.Employees.Edit)]
    public IActionResult Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var updated = _employeeService.Update(id, dto);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Employee with ID {id} not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return BadRequest(new { message = "A database constraint violation occurred while updating the employee." });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AppPolicies.Employees.Delete)]
    public IActionResult Delete(int id)
    {
        bool success = _employeeService.Delete(id);
        if (!success) return NotFound(new { message = $"Employee with ID {id} not found." });
        return Ok(new { message = "Employee deleted successfully." });
    }
}
