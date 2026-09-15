using ERP.Domain.Entities;

namespace ERP.Application.Interfaces.Repositories;

public interface IInventoryRepository
{
    List<Inventory> GetAll();
    Inventory? GetById(int id);
    Inventory? GetByProductId(int productId);
    List<Inventory> GetByProductIds(IEnumerable<int> productIds);
    Inventory Add(Inventory inventory);
    void Update(Inventory inventory);
    void AdjustStock(int productId, int quantityChange, string location = "Main Warehouse");
}
