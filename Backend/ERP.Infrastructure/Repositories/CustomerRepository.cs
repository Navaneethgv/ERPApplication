
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;
public class CustomerRepository : ICustomerRepository
{
    private readonly ErpDbContext _context;

    public CustomerRepository(ErpDbContext context)
    {
        _context = context;
    }

    public List<Customer> GetAll()
    {
        return _context.Customers.AsNoTracking().ToList();
    }

    public Customer? GetById(int id)
    {
        return _context.Customers.AsNoTracking().FirstOrDefault(c => c.CustomerId == id);
    }

    public Customer? GetByEmail(string email)
    {
        var lowerEmail = email.ToLower();
        return _context.Customers.AsNoTracking().FirstOrDefault(c => c.Email.ToLower() == lowerEmail);
    }

    public Customer Add(Customer customer)
    {
        _context.Customers.Add(customer);
        _context.SaveChanges();
        return customer;
    }

    public void Update(Customer customer)
    {
        var existing = _context.Customers.Find(customer.CustomerId);
        if (existing != null)
        {
            existing.Name = customer.Name;
            existing.ContactPerson = customer.ContactPerson;
            existing.Email = customer.Email;
            existing.Phone = customer.Phone;
            existing.Company = customer.Company;
            existing.Address = customer.Address;
            existing.CreditLimit = customer.CreditLimit;
            existing.CurrentBalance = customer.CurrentBalance;
            existing.Status = customer.Status;
            _context.SaveChanges();
        }
    }

    public void Delete(int id)
    {
        var existing = _context.Customers.Find(id);
        if (existing != null)
        {
            _context.Customers.Remove(existing);
            _context.SaveChanges();
        }
    }
}


