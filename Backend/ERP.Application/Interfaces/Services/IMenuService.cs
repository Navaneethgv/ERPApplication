using ERP.Application.DTOs;

namespace ERP.Application.Interfaces.Services;

public interface IMenuService
{
    List<MenuDto> GetAllMenus();
    MenuDto? GetMenuById(int menuId);
    MenuDto CreateMenu(CreateMenuDto dto);
    MenuDto UpdateMenu(int menuId, UpdateMenuDto dto);
    void DeleteMenu(int menuId);

    MenuOptionDto CreateOption(int menuId, CreateMenuOptionDto dto);
    MenuOptionDto UpdateOption(int optionId, UpdateMenuOptionDto dto);
    void DeleteOption(int optionId);
    List<MenuOptionDto> GetOptionsByMenuId(int menuId);
}
