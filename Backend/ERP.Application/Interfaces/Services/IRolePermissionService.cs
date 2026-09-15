using ERP.Application.DTOs;
using ERP.Domain.Enums;

namespace ERP.Application.Interfaces.Services;

public interface IRolePermissionService
{
    RolePermissionMatrixDto GetMatrixByRole(UserRole role);
    void UpdateRolePermissions(UpdateRolePermissionBatchDto dto);
    UserPermissionsDto GetUserPermissions(UserRole role);
    bool HasPermission(UserRole role, string menuRoute, string optionCode);
}
