
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;
public class ProductRepository : IProductRepository
{
    private readonly ErpDbContext _context;

    public ProductRepository(ErpDbContext context)
    {
        _context = context;
    }

    public List<Product> GetAll()
    {
        return _context.Products.AsNoTracking().ToList();
    }

    public Product? GetById(int id)
    {
        return _context.Products.AsNoTracking().FirstOrDefault(p => p.ProductId == id);
    }

    public List<Product> GetByIds(IEnumerable<int> ids)
    {
        var idList = ids.Distinct().ToList();
        if (!idList.Any()) return new List<Product>();

        return _context.Products
            .AsNoTracking()
            .Where(p => idList.Contains(p.ProductId))
            .ToList();
    }

    public Product? GetBySKU(string sku)
    {
        var lowerSku = sku.ToLower();
        return _context.Products.AsNoTracking().FirstOrDefault(p => p.SKU.ToLower() == lowerSku);
    }

    public Product Add(Product product)
    {
        _context.Products.Add(product);
        _context.SaveChanges();
        return product;
    }

    public void Update(Product product)
    {
        var existing = _context.Products.Find(product.ProductId);
        if (existing != null)
        {
            existing.SKU = product.SKU;
            existing.Name = product.Name;
            existing.Category = product.Category;
            existing.Description = product.Description;
            existing.UnitPrice = product.UnitPrice;
            existing.CostPrice = product.CostPrice;
            existing.UnitOfMeasure = product.UnitOfMeasure;
            existing.ReorderLevel = product.ReorderLevel;
            existing.Status = product.Status;
            _context.SaveChanges();
        }
    }

    public void Delete(int id)
    {
        var existing = _context.Products.Find(id);
        if (existing != null)
        {
            _context.Products.Remove(existing);
            _context.SaveChanges();
        }
    }
}


