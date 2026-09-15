
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;
public class PurchaseRepository : IPurchaseRepository
{
    private readonly ErpDbContext _context;

    public PurchaseRepository(ErpDbContext context)
    {
        _context = context;
    }

    public List<Purchase> GetAll()
    {
        return _context.Purchases
            .Include(p => p.Items)
            .AsNoTracking()
            .OrderByDescending(p => p.PurchaseId)
            .ToList();
    }

    public Purchase? GetById(int id)
    {
        return _context.Purchases
            .Include(p => p.Items)
            .AsNoTracking()
            .FirstOrDefault(p => p.PurchaseId == id);
    }

    public Purchase? GetByNumber(string purchaseNumber)
    {
        var lowerNumber = purchaseNumber.ToLower();
        return _context.Purchases
            .Include(p => p.Items)
            .AsNoTracking()
            .FirstOrDefault(p => p.PurchaseNumber.ToLower() == lowerNumber);
    }

    public Purchase Add(Purchase purchase)
    {
        if (string.IsNullOrEmpty(purchase.PurchaseNumber))
        {
            int nextId = (_context.Purchases.Max(p => (int?)p.PurchaseId) ?? 0) + 1;
            purchase.PurchaseNumber = $"PO-{DateTime.UtcNow:yyyy}-{nextId:D4}";
        }

        _context.Purchases.Add(purchase);
        _context.SaveChanges();
        return purchase;
    }

    public void Update(Purchase purchase)
    {
        var existing = _context.Purchases.Find(purchase.PurchaseId);
        if (existing != null)
        {
            existing.SupplierId = purchase.SupplierId;
            existing.SupplierName = purchase.SupplierName;
            existing.PurchaseDate = purchase.PurchaseDate;
            existing.ExpectedDeliveryDate = purchase.ExpectedDeliveryDate;
            existing.TotalAmount = purchase.TotalAmount;
            existing.Status = purchase.Status;
            existing.Notes = purchase.Notes;
            _context.SaveChanges();
        }
    }

    public void Delete(int id)
    {
        var existing = _context.Purchases.Include(p => p.Items).FirstOrDefault(p => p.PurchaseId == id);
        if (existing != null)
        {
            _context.Purchases.Remove(existing);
            _context.SaveChanges();
        }
    }
}


