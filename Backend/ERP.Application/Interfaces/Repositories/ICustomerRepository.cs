using ERP.Domain.Entities;

namespace ERP.Application.Interfaces.Repositories;

public interface ICustomerRepository
{
    List<Customer> GetAll();
    Customer? GetById(int id);
    Customer? GetByEmail(string email);
    Customer Add(Customer customer);
    void Update(Customer customer);
    void Delete(int id);
}
