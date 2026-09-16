using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using ERP.Application.DTOs;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Services;
using ERP.Domain.Common;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ERP.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPasswordPolicyService _passwordPolicyService;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        ICustomerRepository customerRepository,
        IPasswordPolicyService passwordPolicyService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _customerRepository = customerRepository;
        _passwordPolicyService = passwordPolicyService;
        _configuration = configuration;
    }

    public LoginResponseDto? Login(LoginRequestDto request)
    {
        var user = _userRepository.GetByUsername(request.Username);
        if (user == null || user.Status != RecordStatus.Active)
        {
            return null;
        }

        var policy = _passwordPolicyService.GetPolicy();

        // 1. Account Lockout Check
        if (user.LockoutEnd.HasValue)
        {
            if (user.LockoutEnd.Value > DateTime.UtcNow)
            {
                var remainingMinutes = Math.Max(1, (int)Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes));
                throw new InvalidOperationException($"Account is temporarily locked due to failed login attempts. Please try again after {remainingMinutes} minute(s).");
            }
            else
            {
                // Lockout has elapsed, reset counter
                user.LockoutEnd = null;
                user.FailedLoginAttempts = 0;
                _userRepository.Update(user);
            }
        }

        // 2. Password Verification
        bool validPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!validPassword)
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= policy.MaxFailedAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(policy.LockoutDurationMinutes);
                _userRepository.Update(user);
                throw new InvalidOperationException($"Account locked for {policy.LockoutDurationMinutes} minute(s) due to {policy.MaxFailedAttempts} consecutive failed login attempts.");
            }
            _userRepository.Update(user);
            return null;
        }

        // 3. Reset failed attempts on successful authentication
        if (user.FailedLoginAttempts > 0 || user.LockoutEnd != null)
        {
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            _userRepository.Update(user);
        }

        // 4. Password Expiry Check
        bool isExpired = false;
        if (policy.ExpiryDays > 0)
        {
            isExpired = user.PasswordChangedAt.AddDays(policy.ExpiryDays) < DateTime.UtcNow;
        }

        // 5. Generate JWT Token
        var tokenHandler = new JwtSecurityTokenHandler();
        var configKey = _configuration["JwtSettings:Key"];
        var jwtKey = string.IsNullOrWhiteSpace(configKey)
            ? "ERP_Secure_JWT_Secret_Key_2026_Enterprise_System_Super_Secret_Key_!#9988"
            : configKey;
        var key = Encoding.UTF8.GetBytes(jwtKey);
        var expiryMinutes = int.TryParse(_configuration["JwtSettings:ExpiryInMinutes"], out int exp) ? exp : 480;
        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("FullName", user.FullName),
            new("Status", user.Status.ToString())
        };

        if (user.EmployeeId.HasValue)
        {
            claims.Add(new Claim("EmployeeId", user.EmployeeId.Value.ToString()));
        }
        if (user.CustomerId.HasValue)
        {
            claims.Add(new Claim("CustomerId", user.CustomerId.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _configuration["JwtSettings:Issuer"] ?? "ERPApplicationServer",
            Audience = _configuration["JwtSettings:Audience"] ?? "ERPApplicationClient",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new LoginResponseDto
        {
            Token = tokenString,
            Expiration = expires,
            PasswordExpired = isExpired,
            User = new UserProfileDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role.ToString(),
                EmployeeId = user.EmployeeId,
                CustomerId = user.CustomerId,
                FullName = user.FullName,
                Status = user.Status.ToString(),
                PasswordChangedAt = user.PasswordChangedAt
            }
        };
    }

    public UserProfileDto? GetUserProfile(int userId)
    {
        var user = _userRepository.GetById(userId);
        if (user == null) return null;

        return new UserProfileDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Role = user.Role.ToString(),
            EmployeeId = user.EmployeeId,
            CustomerId = user.CustomerId,
            FullName = user.FullName,
            Status = user.Status.ToString(),
            PasswordChangedAt = user.PasswordChangedAt
        };
    }

    public bool ChangePassword(int userId, ChangePasswordDto dto)
    {
        var user = _userRepository.GetById(userId);
        if (user == null) return false;

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }

        // Validate new password against policy and password history
        var validation = _passwordPolicyService.ValidatePassword(dto.NewPassword, userId);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(string.Join(" ", validation.Errors));
        }

        // Save current password in history before updating
        _passwordPolicyService.RecordPasswordHistory(userId, user.PasswordHash);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.PasswordChangedAt = TimeHelper.Now;
        _userRepository.Update(user);
        return true;
    }

    public UserProfileDto RegisterCustomer(RegisterCustomerDto dto)
    {
        var existingUser = _userRepository.GetByUsername(dto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("An account with this email address already exists.");
        }

        // Validate password against policy
        var validation = _passwordPolicyService.ValidatePassword(dto.Password);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(string.Join(" ", validation.Errors));
        }

        // 1. Create Customer record
        var customer = new Customer
        {
            Name = dto.Name,
            ContactPerson = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Company = string.IsNullOrEmpty(dto.Company) ? dto.Name : dto.Company,
            Address = dto.Address,
            CreditLimit = 10000m,
            CurrentBalance = 0m,
            Status = RecordStatus.Active,
            CreatedAt = TimeHelper.Now
        };
        customer = _customerRepository.Add(customer);

        // 2. Create User record
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var user = new User
        {
            Username = dto.Email,
            PasswordHash = passwordHash,
            Role = UserRole.Customer,
            CustomerId = customer.CustomerId,
            FullName = dto.Name,
            Status = RecordStatus.Active,
            CreatedAt = TimeHelper.Now,
            PasswordChangedAt = TimeHelper.Now,
            FailedLoginAttempts = 0,
            LockoutEnd = null
        };
        user = _userRepository.Add(user);

        // 3. Record initial password in history
        _passwordPolicyService.RecordPasswordHistory(user.UserId, passwordHash);

        return new UserProfileDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Role = user.Role.ToString(),
            CustomerId = user.CustomerId,
            FullName = user.FullName,
            Status = user.Status.ToString(),
            PasswordChangedAt = user.PasswordChangedAt
        };
    }
}
