using ERP.Application.DTOs;

namespace ERP.Application.Interfaces.Services;

public interface ISalesService
{
    List<SaleDto> GetAll(int? customerId = null);
    SaleDto? GetById(int id, int? customerId = null);
    SaleDto Create(CreateSaleDto dto, int? customerIdFromToken, string createdBy);
    SaleDto UpdateStatus(int id, UpdateSaleStatusDto dto, string user);
    bool Delete(int id);
}
