namespace ERP.Domain.Entities;

public class MenuOption
{
    public int MenuOptionId { get; set; }
    public int MenuId { get; set; }
    public string Name { get; set; } = string.Empty; // e.g. "View", "Add", "Edit", "Delete"
    public string Code { get; set; } = string.Empty; // e.g. "VIEW", "ADD", "EDIT", "DELETE"
    public string? Description { get; set; }
    public int SortOrder { get; set; } = 0;

    public virtual Menu Menu { get; set; } = null!;
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
