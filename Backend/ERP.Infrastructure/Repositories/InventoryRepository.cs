

using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;
public class InventoryRepository : IInventoryRepository
{
    private readonly ErpDbContext _context;

    public InventoryRepository(ErpDbContext context)
    {
        _context = context;
    }

    public List<Inventory> GetAll()
    {
        return _context.Inventory.AsNoTracking().ToList();
    }

    public Inventory? GetById(int id)
    {
        return _context.Inventory.AsNoTracking().FirstOrDefault(i => i.InventoryId == id);
    }

    public Inventory? GetByProductId(int productId)
    {
        return _context.Inventory.AsNoTracking().FirstOrDefault(i => i.ProductId == productId);
    }

    public List<Inventory> GetByProductIds(IEnumerable<int> productIds)
    {
        var idList = productIds.Distinct().ToList();
        if (!idList.Any()) return new List<Inventory>();

        return _context.Inventory
            .AsNoTracking()
            .Where(i => idList.Contains(i.ProductId))
            .ToList();
    }

    public Inventory Add(Inventory inventory)
    {
        _context.Inventory.Add(inventory);
        _context.SaveChanges();
        return inventory;
    }

    public void Update(Inventory inventory)
    {
        var existing = _context.Inventory.Find(inventory.InventoryId);
        if (existing != null)
        {
            existing.ProductId = inventory.ProductId;
            existing.ProductName = inventory.ProductName;
            existing.SKU = inventory.SKU;
            existing.QuantityOnHand = inventory.QuantityOnHand;
            existing.ReservedQuantity = inventory.ReservedQuantity;
            existing.Location = inventory.Location;
            existing.LastUpdated = TimeHelper.Now;
            _context.SaveChanges();
        }
    }

    public void AdjustStock(int productId, int quantityChange, string location = "Main Warehouse")
    {
        var existing = _context.Inventory.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
        {
            existing.QuantityOnHand = Math.Max(0, existing.QuantityOnHand + quantityChange);
            if (!string.IsNullOrEmpty(location))
            {
                existing.Location = location;
            }
            existing.LastUpdated = TimeHelper.Now;
            _context.SaveChanges();
        }
        else
        {
            var product = _context.Products.Find(productId);
            var prodName = product != null ? product.Name : $"Product #{productId}";
            var sku = product != null ? product.SKU : $"SKU-{productId}";

            var newInventory = new Inventory
            {
                ProductId = productId,
                ProductName = prodName,
                SKU = sku,
                QuantityOnHand = Math.Max(0, quantityChange),
                ReservedQuantity = 0,
                Location = !string.IsNullOrEmpty(location) ? location : "Main Warehouse",
                LastUpdated = TimeHelper.Now
            };
            _context.Inventory.Add(newInventory);
            _context.SaveChanges();
        }
    }
}


