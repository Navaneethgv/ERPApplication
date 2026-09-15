using ERP.Application.DTOs;

namespace ERP.Application.Interfaces.Services;

public interface ICustomerService
{
    List<CustomerDto> GetAll();
    CustomerDto? GetById(int id);
    CustomerDto Create(CreateCustomerDto dto);
    CustomerDto Update(int id, UpdateCustomerDto dto);
    bool Delete(int id);
}
