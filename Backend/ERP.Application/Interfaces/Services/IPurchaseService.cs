using ERP.Application.DTOs;

namespace ERP.Application.Interfaces.Services;

public interface IPurchaseService
{
    List<PurchaseDto> GetAll();
    PurchaseDto? GetById(int id);
    PurchaseDto Create(CreatePurchaseDto dto, string createdBy);
    PurchaseDto UpdateStatus(int id, UpdatePurchaseStatusDto dto, string user);
    bool Delete(int id);
}
