
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;
public class InvoiceRepository : IInvoiceRepository
{
    private readonly ErpDbContext _context;

    public InvoiceRepository(ErpDbContext context)
    {
        _context = context;
    }

    public List<Invoice> GetAll()
    {
        return _context.Invoices
            .AsNoTracking()
            .OrderByDescending(i => i.InvoiceId)
            .ToList();
    }

    public Invoice? GetById(int id)
    {
        return _context.Invoices.AsNoTracking().FirstOrDefault(i => i.InvoiceId == id);
    }

    public Invoice? GetByNumber(string invoiceNumber)
    {
        var lowerNumber = invoiceNumber.ToLower();
        return _context.Invoices.AsNoTracking().FirstOrDefault(i => i.InvoiceNumber.ToLower() == lowerNumber);
    }

    public Invoice? GetBySaleId(int saleId)
    {
        return _context.Invoices.AsNoTracking().FirstOrDefault(i => i.SaleId == saleId);
    }

    public List<Invoice> GetByCustomerId(int customerId)
    {
        return _context.Invoices
            .AsNoTracking()
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.InvoiceId)
            .ToList();
    }

    public Invoice Add(Invoice invoice)
    {
        if (string.IsNullOrEmpty(invoice.InvoiceNumber))
        {
            int nextId = (_context.Invoices.Max(i => (int?)i.InvoiceId) ?? 0) + 1;
            invoice.InvoiceNumber = $"INV-{DateTime.UtcNow:yyyy}-{nextId:D4}";
        }

        _context.Invoices.Add(invoice);
        _context.SaveChanges();
        return invoice;
    }

    public void Update(Invoice invoice)
    {
        var existing = _context.Invoices.Find(invoice.InvoiceId);
        if (existing != null)
        {
            existing.DueDate = invoice.DueDate;
            existing.TotalAmount = invoice.TotalAmount;
            existing.PaidAmount = invoice.PaidAmount;
            existing.Status = invoice.Status;
            existing.Notes = invoice.Notes;
            _context.SaveChanges();
        }
    }

    public void Delete(int id)
    {
        var existing = _context.Invoices.Find(id);
        if (existing != null)
        {
            _context.Invoices.Remove(existing);
            _context.SaveChanges();
        }
    }
}


