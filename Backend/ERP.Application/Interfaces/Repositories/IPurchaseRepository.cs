using ERP.Domain.Entities;

namespace ERP.Application.Interfaces.Repositories;

public interface IPurchaseRepository
{
    List<Purchase> GetAll();
    Purchase? GetById(int id);
    Purchase? GetByNumber(string purchaseNumber);
    Purchase Add(Purchase purchase);
    void Update(Purchase purchase);
    void Delete(int id);
}
