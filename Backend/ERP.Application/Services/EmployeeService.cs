



namespace ERP.Application.Services;
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserRepository _userRepository;

    public EmployeeService(IEmployeeRepository employeeRepository, IUserRepository userRepository)
    {
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
    }

    public List<EmployeeDto> GetAll()
    {
        return _employeeRepository.GetAll().Select(MapToDto).ToList();
    }

    public EmployeeDto? GetById(int id)
    {
        var emp = _employeeRepository.GetById(id);
        return emp == null ? null : MapToDto(emp);
    }

    public EmployeeDto Create(CreateEmployeeDto dto)
    {
        var existing = _employeeRepository.GetByEmail(dto.Email);
        if (existing != null)
        {
            throw new InvalidOperationException($"An employee with email '{dto.Email}' already exists.");
        }

        var emp = new Employee
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            Phone = dto.Phone?.Trim() ?? string.Empty,
            Department = dto.Department.Trim(),
            Designation = dto.Designation.Trim(),
            Salary = dto.Salary,
            JoiningDate = dto.JoiningDate,
            Status = Enum.TryParse<RecordStatus>(dto.Status, true, out var st) ? st : RecordStatus.Active
        };

        emp = _employeeRepository.Add(emp);

        if (dto.CreateLoginAccount && !string.IsNullOrEmpty(dto.Password))
        {
            var userExisting = _userRepository.GetByUsername(emp.Email);
            if (userExisting == null)
            {
                _userRepository.Add(new User
                {
                    Username = emp.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = UserRole.Employee,
                    EmployeeId = emp.EmployeeId,
                    FullName = emp.FullName,
                    Status = RecordStatus.Active,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        return MapToDto(emp);
    }

    public EmployeeDto Update(int id, UpdateEmployeeDto dto)
    {
        var emp = _employeeRepository.GetById(id);
        if (emp == null)
        {
            throw new KeyNotFoundException($"Employee with ID {id} not found.");
        }

        var existingEmail = _employeeRepository.GetByEmail(dto.Email);
        if (existingEmail != null && existingEmail.EmployeeId != id)
        {
            throw new InvalidOperationException($"Email '{dto.Email}' is already used by another employee.");
        }

        emp.FirstName = dto.FirstName.Trim();
        emp.LastName = dto.LastName.Trim();
        emp.Email = dto.Email.Trim().ToLowerInvariant();
        emp.Phone = dto.Phone?.Trim() ?? string.Empty;
        emp.Department = dto.Department.Trim();
        emp.Designation = dto.Designation.Trim();
        emp.Salary = dto.Salary;
        emp.JoiningDate = dto.JoiningDate;
        emp.Status = Enum.TryParse<RecordStatus>(dto.Status, true, out var st) ? st : RecordStatus.Active;

        _employeeRepository.Update(emp);

        // Update corresponding User FullName if exists using targeted lookup
        var user = _userRepository.GetByEmployeeId(id);
        if (user != null)
        {
            user.FullName = emp.FullName;
            user.Username = emp.Email;
            _userRepository.Update(user);
        }

        return MapToDto(emp);
    }

    public bool Delete(int id)
    {
        var emp = _employeeRepository.GetById(id);
        if (emp == null) return false;

        _employeeRepository.Delete(id);

        // Delete corresponding user login using targeted lookup
        var user = _userRepository.GetByEmployeeId(id);
        if (user != null)
        {
            _userRepository.Delete(user.UserId);
        }

        return true;
    }

    private static EmployeeDto MapToDto(Employee e) => new()
    {
        EmployeeId = e.EmployeeId,
        FirstName = e.FirstName,
        LastName = e.LastName,
        Email = e.Email,
        Phone = e.Phone,
        Department = e.Department,
        Designation = e.Designation,
        Salary = e.Salary,
        JoiningDate = e.JoiningDate,
        Status = e.Status.ToString()
    };
}

