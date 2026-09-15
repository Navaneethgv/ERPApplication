using ERP.Domain.Common;

namespace ERP.Domain.Entities;

public class Menu
{
    public int MenuId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Route { get; set; }
    public string? Icon { get; set; }
    public int? ParentMenuId { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = TimeHelper.Now;

    public virtual Menu? ParentMenu { get; set; }
    public virtual ICollection<Menu> SubMenus { get; set; } = new List<Menu>();
    public virtual ICollection<MenuOption> MenuOptions { get; set; } = new List<MenuOption>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
