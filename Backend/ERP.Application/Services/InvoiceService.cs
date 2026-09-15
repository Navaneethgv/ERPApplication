



namespace ERP.Application.Services;
public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ITransactionRepository _transactionRepository;

    public InvoiceService(
        IInvoiceRepository invoiceRepository,
        ICustomerRepository customerRepository,
        ITransactionRepository transactionRepository)
    {
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        _transactionRepository = transactionRepository;
    }

    public List<InvoiceDto> GetAll(int? customerId = null)
    {
        var invoices = customerId.HasValue
            ? _invoiceRepository.GetByCustomerId(customerId.Value)
            : _invoiceRepository.GetAll();

        return invoices.Select(MapToDto).ToList();
    }

    public InvoiceDto? GetById(int id, int? customerId = null)
    {
        var inv = _invoiceRepository.GetById(id);
        if (inv == null) return null;

        if (customerId.HasValue && inv.CustomerId != customerId.Value)
        {
            return null; // Forbidden customer access
        }

        return MapToDto(inv);
    }

    public InvoiceDto RecordPayment(int id, RecordPaymentDto dto, string user)
    {
        var invoice = _invoiceRepository.GetById(id);
        if (invoice == null)
        {
            throw new KeyNotFoundException($"Invoice with ID {id} not found.");
        }

        if (invoice.BalanceAmount <= 0)
        {
            throw new InvalidOperationException("This invoice is already fully paid.");
        }

        decimal paymentToApply = Math.Min(dto.Amount, invoice.BalanceAmount);
        invoice.PaidAmount += paymentToApply;

        if (invoice.PaidAmount >= invoice.TotalAmount)
        {
            invoice.Status = InvoiceStatus.Paid;
        }
        else
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }

        _invoiceRepository.Update(invoice);

        // Update customer balance
        var customer = _customerRepository.GetById(invoice.CustomerId);
        if (customer != null)
        {
            customer.CurrentBalance = Math.Max(0, customer.CurrentBalance - paymentToApply);
            _customerRepository.Update(customer);
        }

        // Record payment transaction
        _transactionRepository.Add(new Transaction
        {
            ReferenceType = ReferenceType.InvoicePayment,
            ReferenceNumber = invoice.InvoiceNumber,
            Type = TransactionType.Income,
            Category = "Sales Revenue",
            Amount = paymentToApply,
            TransactionDate = dto.PaymentDate,
            PaymentMethod = dto.PaymentMethod,
            Status = "Completed",
            Notes = $"Payment received for Invoice #{invoice.InvoiceNumber}. {dto.Notes}".Trim(),
            CreatedBy = user
        });

        return MapToDto(invoice);
    }

    private static InvoiceDto MapToDto(Invoice i) => new()
    {
        InvoiceId = i.InvoiceId,
        InvoiceNumber = i.InvoiceNumber,
        SaleId = i.SaleId,
        SaleOrderNumber = i.SaleOrderNumber,
        CustomerId = i.CustomerId,
        CustomerName = i.CustomerName,
        IssueDate = i.IssueDate,
        DueDate = i.DueDate,
        TotalAmount = i.TotalAmount,
        PaidAmount = i.PaidAmount,
        Status = i.Status.ToString(),
        Notes = i.Notes,
        CreatedAt = i.CreatedAt
    };
}

