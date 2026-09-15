
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;
public class TransactionRepository : ITransactionRepository
{
    private readonly ErpDbContext _context;

    public TransactionRepository(ErpDbContext context)
    {
        _context = context;
    }

    public List<Transaction> GetAll()
    {
        return _context.Transactions
            .AsNoTracking()
            .OrderByDescending(t => t.TransactionId)
            .ToList();
    }

    public Transaction? GetById(int id)
    {
        return _context.Transactions.AsNoTracking().FirstOrDefault(t => t.TransactionId == id);
    }

    public Transaction? GetByNumber(string transactionNumber)
    {
        var lowerNumber = transactionNumber.ToLower();
        return _context.Transactions.AsNoTracking().FirstOrDefault(t => t.TransactionNumber.ToLower() == lowerNumber);
    }

    public List<Transaction> GetByReferenceNumber(string refNumber)
    {
        var lowerRef = refNumber.ToLower();
        return _context.Transactions
            .AsNoTracking()
            .Where(t => t.ReferenceNumber.ToLower() == lowerRef)
            .OrderByDescending(t => t.TransactionId)
            .ToList();
    }

    public List<Transaction> GetByReferenceNumbers(IEnumerable<string> refNumbers)
    {
        var list = refNumbers.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.ToLower()).Distinct().ToList();
        if (!list.Any()) return new List<Transaction>();

        return _context.Transactions
            .AsNoTracking()
            .Where(t => list.Contains(t.ReferenceNumber.ToLower()))
            .OrderByDescending(t => t.TransactionId)
            .ToList();
    }

    public Transaction Add(Transaction transaction)
    {
        if (string.IsNullOrEmpty(transaction.TransactionNumber))
        {
            int nextId = (_context.Transactions.Max(t => (int?)t.TransactionId) ?? 0) + 1;
            transaction.TransactionNumber = $"TXN-{DateTime.UtcNow:yyyy}-{nextId:D4}";
        }

        _context.Transactions.Add(transaction);
        _context.SaveChanges();
        return transaction;
    }

    public void Update(Transaction transaction)
    {
        var existing = _context.Transactions.Find(transaction.TransactionId);
        if (existing != null)
        {
            existing.ReferenceType = transaction.ReferenceType;
            existing.ReferenceNumber = transaction.ReferenceNumber;
            existing.Type = transaction.Type;
            existing.Category = transaction.Category;
            existing.Amount = transaction.Amount;
            existing.TransactionDate = transaction.TransactionDate;
            existing.PaymentMethod = transaction.PaymentMethod;
            existing.Status = transaction.Status;
            existing.Notes = transaction.Notes;
            _context.SaveChanges();
        }
    }

    public void Delete(int id)
    {
        var existing = _context.Transactions.Find(id);
        if (existing != null)
        {
            _context.Transactions.Remove(existing);
            _context.SaveChanges();
        }
    }
}


