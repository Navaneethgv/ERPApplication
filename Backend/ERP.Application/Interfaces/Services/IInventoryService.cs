using ERP.Application.DTOs;

namespace ERP.Application.Interfaces.Services;

public interface IInventoryService
{
    List<InventoryDto> GetAll();
    InventoryDto? GetByProductId(int productId);
    InventoryDto AdjustStock(StockAdjustmentDto dto, string createdBy);
    List<InventoryDto> GetLowStockItems();
}
