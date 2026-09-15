using System.ComponentModel.DataAnnotations;
using ERP.Domain.Enums;

namespace ERP.Application.DTOs;

public class RolePermissionDto
{
    public int RolePermissionId { get; set; }
    public UserRole Role { get; set; }
    public int MenuId { get; set; }
    public string MenuTitle { get; set; } = string.Empty;
    public string? MenuRoute { get; set; }
    public int? MenuOptionId { get; set; }
    public string? OptionName { get; set; }
    public string? OptionCode { get; set; }
    public bool IsGranted { get; set; }
}

public class RolePermissionMatrixItemDto
{
    public int MenuId { get; set; }
    public string MenuTitle { get; set; } = string.Empty;
    public string? MenuRoute { get; set; }
    public int? ParentMenuId { get; set; }
    public int SortOrder { get; set; }
    public List<RolePermissionOptionStateDto> Options { get; set; } = new();
}

public class RolePermissionOptionStateDto
{
    public int MenuOptionId { get; set; }
    public string OptionName { get; set; } = string.Empty;
    public string OptionCode { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
    public int? RolePermissionId { get; set; }
}

public class RolePermissionMatrixDto
{
    public UserRole Role { get; set; }
    public List<RolePermissionMatrixItemDto> Menus { get; set; } = new();
}

public class UpdateRolePermissionBatchDto
{
    [Required]
    public UserRole Role { get; set; }

    public List<RolePermissionEntryDto> Permissions { get; set; } = new();
}

public class RolePermissionEntryDto
{
    public int MenuId { get; set; }
    public int MenuOptionId { get; set; }
    public bool IsGranted { get; set; }
}

public class UserPermissionsDto
{
    public string Role { get; set; } = string.Empty;
    public List<UserNavMenuItemDto> Menus { get; set; } = new();
    public Dictionary<string, List<string>> RouteOptions { get; set; } = new(); // e.g. "products" -> ["VIEW", "ADD", "EDIT"]
}
