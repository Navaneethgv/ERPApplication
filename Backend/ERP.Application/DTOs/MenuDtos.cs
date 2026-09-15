using System.ComponentModel.DataAnnotations;

namespace ERP.Application.DTOs;

public class MenuOptionDto
{
    public int MenuOptionId { get; set; }
    public int MenuId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}

public class CreateMenuOptionDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public int SortOrder { get; set; } = 0;
}

public class UpdateMenuOptionDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public int SortOrder { get; set; } = 0;
}

public class MenuDto
{
    public int MenuId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Route { get; set; }
    public string? Icon { get; set; }
    public int? ParentMenuId { get; set; }
    public string? ParentMenuTitle { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public List<MenuDto> SubMenus { get; set; } = new();
    public List<MenuOptionDto> Options { get; set; } = new();
}

public class CreateMenuDto
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Route { get; set; }

    [MaxLength(100)]
    public string? Icon { get; set; }

    public int? ParentMenuId { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    // Optional list of default option codes to create with this menu (e.g. ["VIEW", "ADD", "EDIT", "DELETE"])
    public List<string>? DefaultOptions { get; set; }
}

public class UpdateMenuDto
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Route { get; set; }

    [MaxLength(100)]
    public string? Icon { get; set; }

    public int? ParentMenuId { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}

public class UserNavMenuItemDto
{
    public int MenuId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Route { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public List<UserNavMenuItemDto> SubMenus { get; set; } = new();
    public List<string> GrantedOptionCodes { get; set; } = new();
}
