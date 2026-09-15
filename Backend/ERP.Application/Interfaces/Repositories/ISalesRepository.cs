using ERP.Domain.Entities;

namespace ERP.Application.Interfaces.Repositories;

public interface ISalesRepository
{
    List<Sale> GetAll();
    Sale? GetById(int id);
    Sale? GetByNumber(string saleOrderNumber);
    List<Sale> GetByCustomerId(int customerId);
    Sale Add(Sale sale);
    void Update(Sale sale);
    void Delete(int id);
}
