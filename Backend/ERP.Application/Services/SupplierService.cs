



namespace ERP.Application.Services;
public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _supplierRepository;

    public SupplierService(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public List<SupplierDto> GetAll()
    {
        return _supplierRepository.GetAll().Select(MapToDto).ToList();
    }

    public SupplierDto? GetById(int id)
    {
        var s = _supplierRepository.GetById(id);
        return s == null ? null : MapToDto(s);
    }

    public SupplierDto Create(CreateSupplierDto dto)
    {
        var existing = _supplierRepository.GetByCode(dto.SupplierCode);
        if (existing != null)
        {
            throw new InvalidOperationException($"Supplier code '{dto.SupplierCode}' already exists.");
        }

        var supplier = new Supplier
        {
            SupplierCode = dto.SupplierCode.Trim().ToUpperInvariant(),
            Name = dto.Name.Trim(),
            ContactPerson = dto.ContactPerson?.Trim() ?? string.Empty,
            Email = dto.Email.Trim().ToLowerInvariant(),
            Phone = dto.Phone?.Trim() ?? string.Empty,
            Address = dto.Address?.Trim() ?? string.Empty,
            PaymentTerms = string.IsNullOrEmpty(dto.PaymentTerms) ? "Net 30" : dto.PaymentTerms.Trim(),
            Status = Enum.TryParse<RecordStatus>(dto.Status, true, out var st) ? st : RecordStatus.Active,
            CreatedAt = TimeHelper.Now
        };

        supplier = _supplierRepository.Add(supplier);
        return MapToDto(supplier);
    }

    public SupplierDto Update(int id, UpdateSupplierDto dto)
    {
        var supplier = _supplierRepository.GetById(id);
        if (supplier == null)
        {
            throw new KeyNotFoundException($"Supplier with ID {id} not found.");
        }

        var existingCode = _supplierRepository.GetByCode(dto.SupplierCode);
        if (existingCode != null && existingCode.SupplierId != id)
        {
            throw new InvalidOperationException($"Supplier code '{dto.SupplierCode}' is already used by another supplier.");
        }

        supplier.SupplierCode = dto.SupplierCode.Trim().ToUpperInvariant();
        supplier.Name = dto.Name.Trim();
        supplier.ContactPerson = dto.ContactPerson?.Trim() ?? string.Empty;
        supplier.Email = dto.Email.Trim().ToLowerInvariant();
        supplier.Phone = dto.Phone?.Trim() ?? string.Empty;
        supplier.Address = dto.Address?.Trim() ?? string.Empty;
        supplier.PaymentTerms = string.IsNullOrEmpty(dto.PaymentTerms) ? "Net 30" : dto.PaymentTerms.Trim();
        supplier.Status = Enum.TryParse<RecordStatus>(dto.Status, true, out var st) ? st : RecordStatus.Active;

        _supplierRepository.Update(supplier);
        return MapToDto(supplier);
    }

    public bool Delete(int id)
    {
        var supplier = _supplierRepository.GetById(id);
        if (supplier == null) return false;

        _supplierRepository.Delete(id);
        return true;
    }

    private static SupplierDto MapToDto(Supplier s) => new()
    {
        SupplierId = s.SupplierId,
        SupplierCode = s.SupplierCode,
        Name = s.Name,
        ContactPerson = s.ContactPerson,
        Email = s.Email,
        Phone = s.Phone,
        Address = s.Address,
        PaymentTerms = s.PaymentTerms,
        Status = s.Status.ToString(),
        CreatedAt = s.CreatedAt
    };
}


