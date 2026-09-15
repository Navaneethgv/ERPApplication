using ERP.Application.Interfaces.Repositories;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly ApplicationDbContext _context;

    public RolePermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<RolePermission> GetPermissionsByRole(UserRole role)
    {
        return _context.RolePermissions
            .Include(p => p.Menu)
            .Include(p => p.MenuOption)
            .Where(p => p.Role == role)
            .ToList();
    }

    public List<RolePermission> GetAllPermissions()
    {
        return _context.RolePermissions
            .Include(p => p.Menu)
            .Include(p => p.MenuOption)
            .ToList();
    }

    public void SaveRolePermissions(UserRole role, List<(int menuId, int menuOptionId, bool isGranted)> permissions)
    {
        var existing = _context.RolePermissions
            .Where(p => p.Role == role)
            .ToList();

        var existingMap = existing
            .Where(p => p.MenuOptionId.HasValue)
            .ToDictionary(p => (p.MenuId, p.MenuOptionId!.Value));

        foreach (var (menuId, menuOptionId, isGranted) in permissions)
        {
            if (existingMap.TryGetValue((menuId, menuOptionId), out var perm))
            {
                perm.IsGranted = isGranted;
            }
            else
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    Role = role,
                    MenuId = menuId,
                    MenuOptionId = menuOptionId,
                    IsGranted = isGranted
                });
            }
        }

        _context.SaveChanges();
    }

    public bool HasPermission(UserRole role, string menuRoute, string optionCode)
    {
        if (role == UserRole.Admin) return true;

        var routeNormalized = menuRoute.Trim().ToLowerInvariant();
        var codeNormalized = optionCode.Trim().ToUpperInvariant();

        return _context.RolePermissions
            .Any(p => p.Role == role &&
                      p.IsGranted &&
                      p.Menu.Route != null &&
                      p.Menu.Route.ToLower() == routeNormalized &&
                      p.MenuOption != null &&
                      p.MenuOption.Code.ToUpper() == codeNormalized);
    }
}
