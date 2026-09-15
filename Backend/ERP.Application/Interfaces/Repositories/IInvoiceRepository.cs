using ERP.Domain.Entities;

namespace ERP.Application.Interfaces.Repositories;

public interface IInvoiceRepository
{
    List<Invoice> GetAll();
    Invoice? GetById(int id);
    Invoice? GetByNumber(string invoiceNumber);
    Invoice? GetBySaleId(int saleId);
    List<Invoice> GetByCustomerId(int customerId);
    Invoice Add(Invoice invoice);
    void Update(Invoice invoice);
    void Delete(int id);
}
