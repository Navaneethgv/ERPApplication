using ERP.Application.DTOs;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Services;
using ERP.Domain.Entities;
using ERP.Domain.Enums;

namespace ERP.Application.Services;

public class RolePermissionService : IRolePermissionService
{
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IMenuRepository _menuRepository;

    public RolePermissionService(
        IRolePermissionRepository rolePermissionRepository,
        IMenuRepository menuRepository)
    {
        _rolePermissionRepository = rolePermissionRepository;
        _menuRepository = menuRepository;
    }

    public RolePermissionMatrixDto GetMatrixByRole(UserRole role)
    {
        var allMenus = _menuRepository.GetAllMenusWithDetails();
        var existingPerms = _rolePermissionRepository.GetPermissionsByRole(role);

        var permLookup = existingPerms
            .Where(p => p.MenuOptionId.HasValue)
            .ToDictionary(p => (p.MenuId, p.MenuOptionId!.Value));

        var items = new List<RolePermissionMatrixItemDto>();

        foreach (var menu in allMenus.OrderBy(m => m.ParentMenuId.HasValue ? 1 : 0).ThenBy(m => m.SortOrder))
        {
            var item = new RolePermissionMatrixItemDto
            {
                MenuId = menu.MenuId,
                MenuTitle = menu.Title,
                MenuRoute = menu.Route,
                ParentMenuId = menu.ParentMenuId,
                SortOrder = menu.SortOrder,
                Options = menu.MenuOptions.OrderBy(o => o.SortOrder).Select(opt =>
                {
                    bool granted = false;
                    int? permId = null;

                    if (permLookup.TryGetValue((menu.MenuId, opt.MenuOptionId), out var p))
                    {
                        granted = p.IsGranted;
                        permId = p.RolePermissionId;
                    }
                    else if (role == UserRole.Admin)
                    {
                        // Admin defaults to true if not explicitly set
                        granted = true;
                    }

                    return new RolePermissionOptionStateDto
                    {
                        MenuOptionId = opt.MenuOptionId,
                        OptionName = opt.Name,
                        OptionCode = opt.Code,
                        IsGranted = granted,
                        RolePermissionId = permId
                    };
                }).ToList()
            };

            items.Add(item);
        }

        return new RolePermissionMatrixDto
        {
            Role = role,
            Menus = items
        };
    }

    public void UpdateRolePermissions(UpdateRolePermissionBatchDto dto)
    {
        var tuples = dto.Permissions.Select(p => (p.MenuId, p.MenuOptionId, p.IsGranted)).ToList();
        _rolePermissionRepository.SaveRolePermissions(dto.Role, tuples);
    }

    public UserPermissionsDto GetUserPermissions(UserRole role)
    {
        var allMenus = _menuRepository.GetAllMenusWithDetails();
        var rolePerms = _rolePermissionRepository.GetPermissionsByRole(role);

        // A menu is accessible if it has at least one granted option (e.g. VIEW),
        // or if it is a parent menu that has accessible submenus.
        var grantedLookup = rolePerms
            .Where(p => p.IsGranted && p.MenuOptionId.HasValue)
            .GroupBy(p => p.MenuId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => p.MenuOption?.Code ?? string.Empty).Where(c => !string.IsNullOrEmpty(c)).ToList()
            );

        // Admin has all permissions by default if empty
        if (role == UserRole.Admin && !grantedLookup.Any())
        {
            foreach (var m in allMenus)
            {
                grantedLookup[m.MenuId] = m.MenuOptions.Select(o => o.Code).ToList();
            }
        }

        // Build route options map: e.g. "products" -> ["VIEW", "ADD", "EDIT", "DELETE"]
        var routeOptions = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var m in allMenus)
        {
            if (!string.IsNullOrEmpty(m.Route))
            {
                if (grantedLookup.TryGetValue(m.MenuId, out var codes))
                {
                    routeOptions[m.Route] = codes;
                }
                else if (role == UserRole.Admin)
                {
                    routeOptions[m.Route] = m.MenuOptions.Select(o => o.Code).ToList();
                }
            }
        }

        // Build user navigation items
        bool IsMenuAllowed(Menu m)
        {
            if (!m.IsActive) return false;

            // Admin always has access to active menus
            if (role == UserRole.Admin) return true;

            // Check if menu has granted VIEW or any permission
            if (grantedLookup.TryGetValue(m.MenuId, out var codes) && codes.Any())
            {
                return true;
            }

            // Check if any child submenu is allowed
            var children = allMenus.Where(c => c.ParentMenuId == m.MenuId).ToList();
            return children.Any(IsMenuAllowed);
        }

        UserNavMenuItemDto BuildUserNav(Menu m)
        {
            var childNavs = allMenus
                .Where(c => c.ParentMenuId == m.MenuId && IsMenuAllowed(c))
                .OrderBy(c => c.SortOrder)
                .Select(BuildUserNav)
                .ToList();

            var codes = grantedLookup.TryGetValue(m.MenuId, out var cList)
                ? cList
                : (role == UserRole.Admin ? m.MenuOptions.Select(o => o.Code).ToList() : new List<string>());

            return new UserNavMenuItemDto
            {
                MenuId = m.MenuId,
                Title = m.Title,
                Route = m.Route,
                Icon = m.Icon,
                SortOrder = m.SortOrder,
                SubMenus = childNavs,
                GrantedOptionCodes = codes
            };
        }

        var topLevelNav = allMenus
            .Where(m => m.ParentMenuId == null && IsMenuAllowed(m))
            .OrderBy(m => m.SortOrder)
            .Select(BuildUserNav)
            .ToList();

        return new UserPermissionsDto
        {
            Role = role.ToString(),
            Menus = topLevelNav,
            RouteOptions = routeOptions
        };
    }

    public bool HasPermission(UserRole role, string menuRoute, string optionCode)
    {
        if (role == UserRole.Admin) return true; // Administrator superuser access
        return _rolePermissionRepository.HasPermission(role, menuRoute, optionCode);
    }
}
