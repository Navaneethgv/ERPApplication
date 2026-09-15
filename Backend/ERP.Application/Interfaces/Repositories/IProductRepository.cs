using ERP.Domain.Entities;

namespace ERP.Application.Interfaces.Repositories;

public interface IProductRepository
{
    List<Product> GetAll();
    Product? GetById(int id);
    List<Product> GetByIds(IEnumerable<int> ids);
    Product? GetBySKU(string sku);
    Product Add(Product product);
    void Update(Product product);
    void Delete(int id);
}
