using ERP.Domain.Entities;

namespace ERP.Application.Interfaces.Repositories;

public interface IMenuRepository
{
    List<Menu> GetAllMenusWithDetails();
    Menu? GetMenuById(int menuId);
    Menu AddMenu(Menu menu);
    void UpdateMenu(Menu menu);
    void DeleteMenu(int menuId);

    MenuOption? GetOptionById(int optionId);
    MenuOption AddOption(MenuOption option);
    void UpdateOption(MenuOption option);
    void DeleteOption(int optionId);
    List<MenuOption> GetOptionsByMenuId(int menuId);
}
