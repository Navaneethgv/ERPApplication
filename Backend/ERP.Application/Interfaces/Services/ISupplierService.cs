using ERP.Application.DTOs;

namespace ERP.Application.Interfaces.Services;

public interface ISupplierService
{
    List<SupplierDto> GetAll();
    SupplierDto? GetById(int id);
    SupplierDto Create(CreateSupplierDto dto);
    SupplierDto Update(int id, UpdateSupplierDto dto);
    bool Delete(int id);
}
