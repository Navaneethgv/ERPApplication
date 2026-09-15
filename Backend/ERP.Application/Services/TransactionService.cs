



namespace ERP.Application.Services;
public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IInvoiceRepository invoiceRepository)
    {
        _transactionRepository = transactionRepository;
        _invoiceRepository = invoiceRepository;
    }

    public List<TransactionDto> GetAll(int? customerId = null)
    {
        var allTxns = _transactionRepository.GetAll();

        if (customerId.HasValue)
        {
            // Filter transactions related to this customer's invoices
            var customerInvoices = _invoiceRepository.GetByCustomerId(customerId.Value)
                .Select(i => i.InvoiceNumber)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return allTxns.Where(t => !string.IsNullOrEmpty(t.ReferenceNumber) && customerInvoices.Contains(t.ReferenceNumber))
                          .Select(MapToDto).ToList();
        }

        return allTxns.Select(MapToDto).ToList();
    }

    public TransactionDto? GetById(int id)
    {
        var t = _transactionRepository.GetById(id);
        return t == null ? null : MapToDto(t);
    }

    public TransactionDto Create(CreateTransactionDto dto, string createdBy)
    {
        var transaction = new Transaction
        {
            ReferenceType = dto.ReferenceType,
            ReferenceNumber = dto.ReferenceNumber?.Trim() ?? string.Empty,
            Type = dto.Type,
            Category = dto.Category.Trim(),
            Amount = dto.Amount,
            TransactionDate = dto.TransactionDate,
            PaymentMethod = dto.PaymentMethod,
            Status = "Completed",
            Notes = dto.Notes?.Trim() ?? string.Empty,
            CreatedBy = createdBy
        };

        transaction = _transactionRepository.Add(transaction);
        return MapToDto(transaction);
    }

    private static TransactionDto MapToDto(Transaction t) => new()
    {
        TransactionId = t.TransactionId,
        TransactionNumber = t.TransactionNumber,
        ReferenceType = t.ReferenceType.ToString(),
        ReferenceNumber = t.ReferenceNumber,
        Type = t.Type.ToString(),
        Category = t.Category,
        Amount = t.Amount,
        TransactionDate = t.TransactionDate,
        PaymentMethod = t.PaymentMethod,
        Status = t.Status,
        Notes = t.Notes,
        CreatedBy = t.CreatedBy
    };
}

