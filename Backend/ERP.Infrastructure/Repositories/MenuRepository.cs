using ERP.Application.Interfaces.Repositories;
using ERP.Domain.Entities;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;

public class MenuRepository : IMenuRepository
{
    private readonly ApplicationDbContext _context;

    public MenuRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Menu> GetAllMenusWithDetails()
    {
        return _context.Menus
            .AsSplitQuery()
            .Include(m => m.ParentMenu)
            .Include(m => m.MenuOptions)
            .Include(m => m.RolePermissions)
            .OrderBy(m => m.SortOrder)
            .ToList();
    }

    public Menu? GetMenuById(int menuId)
    {
        return _context.Menus
            .AsSplitQuery()
            .Include(m => m.ParentMenu)
            .Include(m => m.MenuOptions)
            .Include(m => m.RolePermissions)
            .FirstOrDefault(m => m.MenuId == menuId);
    }

    public Menu AddMenu(Menu menu)
    {
        _context.Menus.Add(menu);
        _context.SaveChanges();
        return menu;
    }

    public void UpdateMenu(Menu menu)
    {
        _context.Menus.Update(menu);
        _context.SaveChanges();
    }

    public void DeleteMenu(int menuId)
    {
        var menu = _context.Menus
            .Include(m => m.SubMenus)
            .FirstOrDefault(m => m.MenuId == menuId);

        if (menu != null)
        {
            // If it has submenus, unparent them
            foreach (var sub in menu.SubMenus)
            {
                sub.ParentMenuId = null;
            }
            _context.Menus.Remove(menu);
            _context.SaveChanges();
        }
    }

    public MenuOption? GetOptionById(int optionId)
    {
        return _context.MenuOptions.Find(optionId);
    }

    public MenuOption AddOption(MenuOption option)
    {
        _context.MenuOptions.Add(option);
        _context.SaveChanges();
        return option;
    }

    public void UpdateOption(MenuOption option)
    {
        _context.MenuOptions.Update(option);
        _context.SaveChanges();
    }

    public void DeleteOption(int optionId)
    {
        var opt = _context.MenuOptions.Find(optionId);
        if (opt != null)
        {
            _context.MenuOptions.Remove(opt);
            _context.SaveChanges();
        }
    }

    public List<MenuOption> GetOptionsByMenuId(int menuId)
    {
        return _context.MenuOptions
            .Where(o => o.MenuId == menuId)
            .OrderBy(o => o.SortOrder)
            .ToList();
    }
}
