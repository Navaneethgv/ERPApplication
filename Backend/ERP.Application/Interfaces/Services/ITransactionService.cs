using ERP.Application.DTOs;

namespace ERP.Application.Interfaces.Services;

public interface ITransactionService
{
    List<TransactionDto> GetAll(int? customerId = null);
    TransactionDto? GetById(int id);
    TransactionDto Create(CreateTransactionDto dto, string createdBy);
}
