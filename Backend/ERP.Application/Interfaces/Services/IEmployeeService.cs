using ERP.Application.DTOs;

namespace ERP.Application.Interfaces.Services;

public interface IEmployeeService
{
    List<EmployeeDto> GetAll();
    EmployeeDto? GetById(int id);
    EmployeeDto Create(CreateEmployeeDto dto);
    EmployeeDto Update(int id, UpdateEmployeeDto dto);
    bool Delete(int id);
}
