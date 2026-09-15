
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;
public class SupplierRepository : ISupplierRepository
{
    private readonly ErpDbContext _context;

    public SupplierRepository(ErpDbContext context)
    {
        _context = context;
    }

    public List<Supplier> GetAll()
    {
        return _context.Suppliers.AsNoTracking().ToList();
    }

    public Supplier? GetById(int id)
    {
        return _context.Suppliers.AsNoTracking().FirstOrDefault(s => s.SupplierId == id);
    }

    public Supplier? GetByCode(string code)
    {
        var lowerCode = code.ToLower();
        return _context.Suppliers.AsNoTracking().FirstOrDefault(s => s.SupplierCode.ToLower() == lowerCode);
    }

    public Supplier Add(Supplier supplier)
    {
        _context.Suppliers.Add(supplier);
        _context.SaveChanges();
        return supplier;
    }

    public void Update(Supplier supplier)
    {
        var existing = _context.Suppliers.Find(supplier.SupplierId);
        if (existing != null)
        {
            existing.SupplierCode = supplier.SupplierCode;
            existing.Name = supplier.Name;
            existing.ContactPerson = supplier.ContactPerson;
            existing.Email = supplier.Email;
            existing.Phone = supplier.Phone;
            existing.Address = supplier.Address;
            existing.PaymentTerms = supplier.PaymentTerms;
            existing.Status = supplier.Status;
            _context.SaveChanges();
        }
    }

    public void Delete(int id)
    {
        var existing = _context.Suppliers.Find(id);
        if (existing != null)
        {
            _context.Suppliers.Remove(existing);
            _context.SaveChanges();
        }
    }
}


