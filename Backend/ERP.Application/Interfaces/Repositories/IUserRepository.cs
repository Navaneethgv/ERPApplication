using ERP.Domain.Entities;

namespace ERP.Application.Interfaces.Repositories;

public interface IUserRepository
{
    List<User> GetAll();
    User? GetById(int id);
    User? GetByUsername(string username);
    User? GetByEmployeeId(int employeeId);
    User? GetByCustomerId(int customerId);
    User Add(User user);
    void Update(User user);
    void Delete(int id);
}
