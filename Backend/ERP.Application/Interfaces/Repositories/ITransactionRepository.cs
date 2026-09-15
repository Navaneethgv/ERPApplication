using ERP.Domain.Entities;

namespace ERP.Application.Interfaces.Repositories;

public interface ITransactionRepository
{
    List<Transaction> GetAll();
    Transaction? GetById(int id);
    Transaction? GetByNumber(string transactionNumber);
    List<Transaction> GetByReferenceNumber(string refNumber);
    List<Transaction> GetByReferenceNumbers(IEnumerable<string> refNumbers);
    Transaction Add(Transaction transaction);
    void Update(Transaction transaction);
    void Delete(int id);
}
