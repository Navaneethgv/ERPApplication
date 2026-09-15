using System.Security.Claims;
using ERP.Application.DTOs;
using ERP.Application.Interfaces.Services;
using ERP.Application.Security;
using ERP.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolePermissionsController : ControllerBase
{
    private readonly IRolePermissionService _rolePermissionService;

    public RolePermissionsController(IRolePermissionService rolePermissionService)
    {
        _rolePermissionService = rolePermissionService;
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.Security.View)]
    public IActionResult GetMatrix([FromQuery] UserRole role = UserRole.Employee)
    {
        var matrix = _rolePermissionService.GetMatrixByRole(role);
        return Ok(matrix);
    }

    [HttpPost("batch-update")]
    [Authorize(Policy = AppPolicies.Security.Edit)]
    public IActionResult BatchUpdate([FromBody] UpdateRolePermissionBatchDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        _rolePermissionService.UpdateRolePermissions(dto);
        return Ok(new { message = $"Permissions for role '{dto.Role}' updated successfully." });
    }

    [HttpGet("my-permissions")]
    [Authorize]
    public IActionResult GetMyPermissions()
    {
        var roleStr = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(roleStr) || !Enum.TryParse<UserRole>(roleStr, true, out var role))
        {
            return Forbid();
        }

        var result = _rolePermissionService.GetUserPermissions(role);
        return Ok(result);
    }
}
