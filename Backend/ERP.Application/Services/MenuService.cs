using ERP.Application.DTOs;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Services;
using ERP.Domain.Common;
using ERP.Domain.Entities;
using ERP.Domain.Enums;

namespace ERP.Application.Services;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;

    public MenuService(IMenuRepository menuRepository, IRolePermissionRepository rolePermissionRepository)
    {
        _menuRepository = menuRepository;
        _rolePermissionRepository = rolePermissionRepository;
    }

    public List<MenuDto> GetAllMenus()
    {
        var allMenus = _menuRepository.GetAllMenusWithDetails();
        var menuDict = allMenus.ToDictionary(m => m.MenuId);

        // Get top-level menus
        var topLevel = allMenus
            .Where(m => m.ParentMenuId == null)
            .OrderBy(m => m.SortOrder)
            .Select(m => MapToDto(m, allMenus))
            .ToList();

        return topLevel;
    }

    public MenuDto? GetMenuById(int menuId)
    {
        var menu = _menuRepository.GetMenuById(menuId);
        if (menu == null) return null;
        var allMenus = _menuRepository.GetAllMenusWithDetails();
        return MapToDto(menu, allMenus);
    }

    public MenuDto CreateMenu(CreateMenuDto dto)
    {
        var menu = new Menu
        {
            Title = dto.Title.Trim(),
            Route = dto.Route?.Trim(),
            Icon = dto.Icon?.Trim(),
            ParentMenuId = dto.ParentMenuId,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            CreatedAt = TimeHelper.Now
        };

        var created = _menuRepository.AddMenu(menu);

        // If default options are provided or default set requested:
        var optionsToCreate = dto.DefaultOptions != null && dto.DefaultOptions.Any()
            ? dto.DefaultOptions
            : new List<string> { "VIEW", "ADD", "EDIT", "DELETE" };

        var newPermissions = new List<(int menuId, int menuOptionId, bool isGranted)>();

        int optSort = 1;
        foreach (var optCode in optionsToCreate)
        {
            string optName = optCode switch
            {
                "VIEW" => "View",
                "ADD" => "Add",
                "EDIT" => "Edit",
                "DELETE" => "Delete",
                _ => optCode
            };

            var opt = _menuRepository.AddOption(new MenuOption
            {
                MenuId = created.MenuId,
                Code = optCode.ToUpperInvariant(),
                Name = optName,
                Description = $"{optName} permission for {created.Title}",
                SortOrder = optSort++
            });

            // Automatically grant Admin permission for newly created menu option
            newPermissions.Add((created.MenuId, opt.MenuOptionId, true));
        }

        if (newPermissions.Any())
        {
            _rolePermissionRepository.SaveRolePermissions(UserRole.Admin, newPermissions);
        }

        return GetMenuById(created.MenuId)!;
    }

    public MenuDto UpdateMenu(int menuId, UpdateMenuDto dto)
    {
        var menu = _menuRepository.GetMenuById(menuId);
        if (menu == null)
        {
            throw new KeyNotFoundException($"Menu with ID {menuId} was not found.");
        }

        menu.Title = dto.Title.Trim();
        menu.Route = dto.Route?.Trim();
        menu.Icon = dto.Icon?.Trim();
        menu.ParentMenuId = dto.ParentMenuId;
        menu.SortOrder = dto.SortOrder;
        menu.IsActive = dto.IsActive;

        _menuRepository.UpdateMenu(menu);
        return GetMenuById(menuId)!;
    }

    public void DeleteMenu(int menuId)
    {
        _menuRepository.DeleteMenu(menuId);
    }

    public MenuOptionDto CreateOption(int menuId, CreateMenuOptionDto dto)
    {
        var menu = _menuRepository.GetMenuById(menuId);
        if (menu == null)
        {
            throw new KeyNotFoundException($"Menu with ID {menuId} was not found.");
        }

        var option = new MenuOption
        {
            MenuId = menuId,
            Name = dto.Name.Trim(),
            Code = dto.Code.Trim().ToUpperInvariant(),
            Description = dto.Description?.Trim(),
            SortOrder = dto.SortOrder
        };

        var created = _menuRepository.AddOption(option);

        // Grant Admin permission by default
        _rolePermissionRepository.SaveRolePermissions(UserRole.Admin, new List<(int, int, bool)>
        {
            (menuId, created.MenuOptionId, true)
        });

        return new MenuOptionDto
        {
            MenuOptionId = created.MenuOptionId,
            MenuId = created.MenuId,
            Name = created.Name,
            Code = created.Code,
            Description = created.Description,
            SortOrder = created.SortOrder
        };
    }

    public MenuOptionDto UpdateOption(int optionId, UpdateMenuOptionDto dto)
    {
        var option = _menuRepository.GetOptionById(optionId);
        if (option == null)
        {
            throw new KeyNotFoundException($"Menu option with ID {optionId} was not found.");
        }

        option.Name = dto.Name.Trim();
        option.Code = dto.Code.Trim().ToUpperInvariant();
        option.Description = dto.Description?.Trim();
        option.SortOrder = dto.SortOrder;

        _menuRepository.UpdateOption(option);

        return new MenuOptionDto
        {
            MenuOptionId = option.MenuOptionId,
            MenuId = option.MenuId,
            Name = option.Name,
            Code = option.Code,
            Description = option.Description,
            SortOrder = option.SortOrder
        };
    }

    public void DeleteOption(int optionId)
    {
        _menuRepository.DeleteOption(optionId);
    }

    public List<MenuOptionDto> GetOptionsByMenuId(int menuId)
    {
        return _menuRepository.GetOptionsByMenuId(menuId)
            .Select(o => new MenuOptionDto
            {
                MenuOptionId = o.MenuOptionId,
                MenuId = o.MenuId,
                Name = o.Name,
                Code = o.Code,
                Description = o.Description,
                SortOrder = o.SortOrder
            })
            .ToList();
    }

    private static MenuDto MapToDto(Menu m, List<Menu> allMenus)
    {
        var subMenus = allMenus
            .Where(s => s.ParentMenuId == m.MenuId)
            .OrderBy(s => s.SortOrder)
            .Select(s => MapToDto(s, allMenus))
            .ToList();

        return new MenuDto
        {
            MenuId = m.MenuId,
            Title = m.Title,
            Route = m.Route,
            Icon = m.Icon,
            ParentMenuId = m.ParentMenuId,
            ParentMenuTitle = m.ParentMenu?.Title,
            SortOrder = m.SortOrder,
            IsActive = m.IsActive,
            SubMenus = subMenus,
            Options = m.MenuOptions
                .OrderBy(o => o.SortOrder)
                .Select(o => new MenuOptionDto
                {
                    MenuOptionId = o.MenuOptionId,
                    MenuId = o.MenuId,
                    Name = o.Name,
                    Code = o.Code,
                    Description = o.Description,
                    SortOrder = o.SortOrder
                }).ToList()
        };
    }
}
