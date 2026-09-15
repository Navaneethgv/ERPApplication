using ERP.Application.DTOs;

namespace ERP.Application.Interfaces.Services;

public interface IProductService
{
    List<ProductDto> GetAll();
    ProductDto? GetById(int id);
    ProductDto Create(CreateProductDto dto);
    ProductDto Update(int id, UpdateProductDto dto);
    bool Delete(int id);
}
