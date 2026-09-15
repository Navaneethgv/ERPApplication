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
[Authorize]
public class MenusController : ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly IRolePermissionService _rolePermissionService;

    public MenusController(IMenuService menuService, IRolePermissionService rolePermissionService)
    {
        _menuService = menuService;
        _rolePermissionService = rolePermissionService;
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.Security.View)]
    public IActionResult GetAll()
    {
        var menus = _menuService.GetAllMenus();
        return Ok(menus);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AppPolicies.Security.View)]
    public IActionResult GetById(int id)
    {
        var menu = _menuService.GetMenuById(id);
        if (menu == null) return NotFound(new { message = $"Menu with ID {id} not found." });
        return Ok(menu);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.Security.Edit)]
    public IActionResult CreateMenu([FromBody] CreateMenuDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = _menuService.CreateMenu(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.MenuId }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AppPolicies.Security.Edit)]
    public IActionResult UpdateMenu(int id, [FromBody] UpdateMenuDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var updated = _menuService.UpdateMenu(id, dto);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AppPolicies.Security.Edit)]
    public IActionResult DeleteMenu(int id)
    {
        _menuService.DeleteMenu(id);
        return NoContent();
    }

    [HttpGet("{menuId}/options")]
    [Authorize(Policy = AppPolicies.Security.View)]
    public IActionResult GetOptions(int menuId)
    {
        var options = _menuService.GetOptionsByMenuId(menuId);
        return Ok(options);
    }

    [HttpPost("{menuId}/options")]
    [Authorize(Policy = AppPolicies.Security.Edit)]
    public IActionResult CreateOption(int menuId, [FromBody] CreateMenuOptionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var created = _menuService.CreateOption(menuId, dto);
            return Ok(created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("options/{optionId}")]
    [Authorize(Policy = AppPolicies.Security.Edit)]
    public IActionResult UpdateOption(int optionId, [FromBody] UpdateMenuOptionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var updated = _menuService.UpdateOption(optionId, dto);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("options/{optionId}")]
    [Authorize(Policy = AppPolicies.Security.Edit)]
    public IActionResult DeleteOption(int optionId)
    {
        _menuService.DeleteOption(optionId);
        return NoContent();
    }

    [HttpGet("user-nav")]
    [Authorize]
    public IActionResult GetUserNav()
    {
        var roleStr = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(roleStr) || !Enum.TryParse<UserRole>(roleStr, true, out var role))
        {
            return Forbid();
        }

        var permissions = _rolePermissionService.GetUserPermissions(role);
        return Ok(permissions);
    }
}
