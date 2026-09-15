using ERP.Domain.Entities;

namespace ERP.Application.Interfaces.Repositories;

public interface ISupplierRepository
{
    List<Supplier> GetAll();
    Supplier? GetById(int id);
    Supplier? GetByCode(string code);
    Supplier Add(Supplier supplier);
    void Update(Supplier supplier);
    void Delete(int id);
}
