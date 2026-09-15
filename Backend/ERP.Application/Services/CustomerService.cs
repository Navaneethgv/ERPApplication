



namespace ERP.Application.Services;
public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUserRepository _userRepository;

    public CustomerService(ICustomerRepository customerRepository, IUserRepository userRepository)
    {
        _customerRepository = customerRepository;
        _userRepository = userRepository;
    }

    public List<CustomerDto> GetAll()
    {
        return _customerRepository.GetAll().Select(MapToDto).ToList();
    }

    public CustomerDto? GetById(int id)
    {
        var cust = _customerRepository.GetById(id);
        return cust == null ? null : MapToDto(cust);
    }

    public CustomerDto Create(CreateCustomerDto dto)
    {
        var existing = _customerRepository.GetByEmail(dto.Email);
        if (existing != null)
        {
            throw new InvalidOperationException($"A customer with email '{dto.Email}' already exists.");
        }

        var cust = new Customer
        {
            Name = dto.Name.Trim(),
            ContactPerson = dto.ContactPerson?.Trim() ?? string.Empty,
            Email = dto.Email.Trim().ToLowerInvariant(),
            Phone = dto.Phone?.Trim() ?? string.Empty,
            Company = string.IsNullOrEmpty(dto.Company) ? dto.Name.Trim() : dto.Company.Trim(),
            Address = dto.Address?.Trim() ?? string.Empty,
            CreditLimit = dto.CreditLimit,
            CurrentBalance = 0m,
            Status = Enum.TryParse<RecordStatus>(dto.Status, true, out var st) ? st : RecordStatus.Active,
            CreatedAt = TimeHelper.Now
        };

        cust = _customerRepository.Add(cust);

        if (dto.CreateLoginAccount && !string.IsNullOrEmpty(dto.Password))
        {
            var userExisting = _userRepository.GetByUsername(cust.Email);
            if (userExisting == null)
            {
                _userRepository.Add(new User
                {
                    Username = cust.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = UserRole.Customer,
                    CustomerId = cust.CustomerId,
                    FullName = cust.Name,
                    Status = RecordStatus.Active,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        return MapToDto(cust);
    }

    public CustomerDto Update(int id, UpdateCustomerDto dto)
    {
        var cust = _customerRepository.GetById(id);
        if (cust == null)
        {
            throw new KeyNotFoundException($"Customer with ID {id} not found.");
        }

        var existingEmail = _customerRepository.GetByEmail(dto.Email);
        if (existingEmail != null && existingEmail.CustomerId != id)
        {
            throw new InvalidOperationException($"Email '{dto.Email}' is already used by another customer.");
        }

        cust.Name = dto.Name.Trim();
        cust.ContactPerson = dto.ContactPerson?.Trim() ?? string.Empty;
        cust.Email = dto.Email.Trim().ToLowerInvariant();
        cust.Phone = dto.Phone?.Trim() ?? string.Empty;
        cust.Company = dto.Company?.Trim() ?? string.Empty;
        cust.Address = dto.Address?.Trim() ?? string.Empty;
        cust.CreditLimit = dto.CreditLimit;
        cust.Status = Enum.TryParse<RecordStatus>(dto.Status, true, out var st) ? st : RecordStatus.Active;

        _customerRepository.Update(cust);

        // Update corresponding User FullName if exists using targeted lookup
        var user = _userRepository.GetByCustomerId(id);
        if (user != null)
        {
            user.FullName = cust.Name;
            user.Username = cust.Email;
            _userRepository.Update(user);
        }

        return MapToDto(cust);
    }

    public bool Delete(int id)
    {
        var cust = _customerRepository.GetById(id);
        if (cust == null) return false;

        _customerRepository.Delete(id);

        // Delete corresponding user login using targeted lookup
        var user = _userRepository.GetByCustomerId(id);
        if (user != null)
        {
            _userRepository.Delete(user.UserId);
        }

        return true;
    }

    private static CustomerDto MapToDto(Customer c) => new()
    {
        CustomerId = c.CustomerId,
        Name = c.Name,
        ContactPerson = c.ContactPerson,
        Email = c.Email,
        Phone = c.Phone,
        Company = c.Company,
        Address = c.Address,
        CreditLimit = c.CreditLimit,
        CurrentBalance = c.CurrentBalance,
        Status = c.Status.ToString(),
        CreatedAt = c.CreatedAt
    };
}


