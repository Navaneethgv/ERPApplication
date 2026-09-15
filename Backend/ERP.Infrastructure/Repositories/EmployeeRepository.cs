
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;
public class EmployeeRepository : IEmployeeRepository
{
    private readonly ErpDbContext _context;

    public EmployeeRepository(ErpDbContext context)
    {
        _context = context;
    }

    public List<Employee> GetAll()
    {
        return _context.Employees.AsNoTracking().ToList();
    }

    public Employee? GetById(int id)
    {
        return _context.Employees.AsNoTracking().FirstOrDefault(e => e.EmployeeId == id);
    }

    public Employee? GetByEmail(string email)
    {
        var lowerEmail = email.ToLower();
        return _context.Employees.AsNoTracking().FirstOrDefault(e => e.Email.ToLower() == lowerEmail);
    }

    public Employee Add(Employee employee)
    {
        _context.Employees.Add(employee);
        _context.SaveChanges();
        return employee;
    }

    public void Update(Employee employee)
    {
        var existing = _context.Employees.Find(employee.EmployeeId);
        if (existing != null)
        {
            existing.FirstName = employee.FirstName;
            existing.LastName = employee.LastName;
            existing.Email = employee.Email;
            existing.Phone = employee.Phone;
            existing.Department = employee.Department;
            existing.Designation = employee.Designation;
            existing.Salary = employee.Salary;
            existing.JoiningDate = employee.JoiningDate;
            existing.Status = employee.Status;
            _context.SaveChanges();
        }
    }

    public void Delete(int id)
    {
        var existing = _context.Employees.Find(id);
        if (existing != null)
        {
            _context.Employees.Remove(existing);
            _context.SaveChanges();
        }
    }
}


