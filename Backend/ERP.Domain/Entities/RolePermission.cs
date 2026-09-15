using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

public class RolePermission
{
    public int RolePermissionId { get; set; }
    public UserRole Role { get; set; } = UserRole.Employee;
    public int MenuId { get; set; }
    public int? MenuOptionId { get; set; }
    public bool IsGranted { get; set; } = false;

    public virtual Menu Menu { get; set; } = null!;
    public virtual MenuOption? MenuOption { get; set; }
}
