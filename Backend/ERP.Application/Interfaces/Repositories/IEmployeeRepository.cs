using ERP.Domain.Entities;

namespace ERP.Application.Interfaces.Repositories;

public interface IEmployeeRepository
{
    List<Employee> GetAll();
    Employee? GetById(int id);
    Employee? GetByEmail(string email);
    Employee Add(Employee employee);
    void Update(Employee employee);
    void Delete(int id);
}
