using ERP.Application.DTOs;

namespace ERP.Application.Interfaces.Services;

public interface IInvoiceService
{
    List<InvoiceDto> GetAll(int? customerId = null);
    InvoiceDto? GetById(int id, int? customerId = null);
    InvoiceDto RecordPayment(int id, RecordPaymentDto dto, string user);
}
