using ERP.Domain.Entities;
using ERP.Domain.Enums;

namespace ERP.Application.Interfaces.Repositories;

public interface IRolePermissionRepository
{
    List<RolePermission> GetPermissionsByRole(UserRole role);
    List<RolePermission> GetAllPermissions();
    void SaveRolePermissions(UserRole role, List<(int menuId, int menuOptionId, bool isGranted)> permissions);
    bool HasPermission(UserRole role, string menuRoute, string optionCode);
}
