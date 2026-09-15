
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;
public class SalesRepository : ISalesRepository
{
    private readonly ErpDbContext _context;

    public SalesRepository(ErpDbContext context)
    {
        _context = context;
    }

    public List<Sale> GetAll()
    {
        return _context.Sales
            .Include(s => s.Items)
            .AsNoTracking()
            .OrderByDescending(s => s.SaleId)
            .ToList();
    }

    public Sale? GetById(int id)
    {
        return _context.Sales
            .Include(s => s.Items)
            .AsNoTracking()
            .FirstOrDefault(s => s.SaleId == id);
    }

    public Sale? GetByNumber(string saleOrderNumber)
    {
        var lowerNumber = saleOrderNumber.ToLower();
        return _context.Sales
            .Include(s => s.Items)
            .AsNoTracking()
            .FirstOrDefault(s => s.SaleOrderNumber.ToLower() == lowerNumber);
    }

    public List<Sale> GetByCustomerId(int customerId)
    {
        return _context.Sales
            .Include(s => s.Items)
            .AsNoTracking()
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.SaleId)
            .ToList();
    }

    public Sale Add(Sale sale)
    {
        if (string.IsNullOrEmpty(sale.SaleOrderNumber))
        {
            int nextId = (_context.Sales.Max(s => (int?)s.SaleId) ?? 0) + 1;
            sale.SaleOrderNumber = $"SO-{DateTime.UtcNow:yyyy}-{nextId:D4}";
        }

        _context.Sales.Add(sale);
        _context.SaveChanges();
        return sale;
    }

    public void Update(Sale sale)
    {
        var existing = _context.Sales.Find(sale.SaleId);
        if (existing != null)
        {
            existing.CustomerId = sale.CustomerId;
            existing.CustomerName = sale.CustomerName;
            existing.OrderDate = sale.OrderDate;
            existing.DeliveryDate = sale.DeliveryDate;
            existing.TotalAmount = sale.TotalAmount;
            existing.Status = sale.Status;
            existing.Notes = sale.Notes;
            _context.SaveChanges();
        }
    }

    public void Delete(int id)
    {
        var existing = _context.Sales.Include(s => s.Items).FirstOrDefault(s => s.SaleId == id);
        if (existing != null)
        {
            _context.Sales.Remove(existing);
            _context.SaveChanges();
        }
    }
}


