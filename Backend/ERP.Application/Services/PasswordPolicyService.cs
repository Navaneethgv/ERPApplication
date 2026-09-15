using System.Text.RegularExpressions;
using ERP.Application.DTOs;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Services;
using ERP.Domain.Common;

namespace ERP.Application.Services;

public class PasswordPolicyService : IPasswordPolicyService
{
    private readonly IPasswordPolicyRepository _policyRepository;

    public PasswordPolicyService(IPasswordPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public PasswordPolicyDto GetPolicy()
    {
        var p = _policyRepository.GetPolicy();
        return new PasswordPolicyDto
        {
            Id = p.Id,
            MinLength = p.MinLength,
            RequireUppercase = p.RequireUppercase,
            RequireLowercase = p.RequireLowercase,
            RequireDigit = p.RequireDigit,
            RequireSpecialChar = p.RequireSpecialChar,
            ExpiryDays = p.ExpiryDays,
            HistoryCount = p.HistoryCount,
            MaxFailedAttempts = p.MaxFailedAttempts,
            LockoutDurationMinutes = p.LockoutDurationMinutes,
            UpdatedAt = p.UpdatedAt
        };
    }

    public PasswordPolicyDto UpdatePolicy(UpdatePasswordPolicyDto dto)
    {
        var p = _policyRepository.GetPolicy();
        p.MinLength = dto.MinLength;
        p.RequireUppercase = dto.RequireUppercase;
        p.RequireLowercase = dto.RequireLowercase;
        p.RequireDigit = dto.RequireDigit;
        p.RequireSpecialChar = dto.RequireSpecialChar;
        p.ExpiryDays = dto.ExpiryDays;
        p.HistoryCount = dto.HistoryCount;
        p.MaxFailedAttempts = dto.MaxFailedAttempts;
        p.LockoutDurationMinutes = dto.LockoutDurationMinutes;
        p.UpdatedAt = TimeHelper.Now;

        _policyRepository.UpdatePolicy(p);
        return GetPolicy();
    }

    public PasswordValidationResultDto ValidatePassword(string password, int? userId = null)
    {
        var result = new PasswordValidationResultDto { IsValid = true };
        var policy = _policyRepository.GetPolicy();

        if (string.IsNullOrEmpty(password) || password.Length < policy.MinLength)
        {
            result.IsValid = false;
            result.Errors.Add($"Password must be at least {policy.MinLength} characters long.");
        }

        if (policy.RequireUppercase && !password.Any(char.IsUpper))
        {
            result.IsValid = false;
            result.Errors.Add("Password must contain at least one uppercase letter (A-Z).");
        }

        if (policy.RequireLowercase && !password.Any(char.IsLower))
        {
            result.IsValid = false;
            result.Errors.Add("Password must contain at least one lowercase letter (a-z).");
        }

        if (policy.RequireDigit && !password.Any(char.IsDigit))
        {
            result.IsValid = false;
            result.Errors.Add("Password must contain at least one numerical digit (0-9).");
        }

        if (policy.RequireSpecialChar && !Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?~`]"))
        {
            result.IsValid = false;
            result.Errors.Add("Password must contain at least one special character (e.g. !@#$%^&*).");
        }

        // Check password history if user ID provided and history tracking is enabled
        if (userId.HasValue && policy.HistoryCount > 0 && !string.IsNullOrEmpty(password))
        {
            var history = _policyRepository.GetRecentPasswordHistory(userId.Value, policy.HistoryCount);
            foreach (var h in history)
            {
                if (BCrypt.Net.BCrypt.Verify(password, h.PasswordHash))
                {
                    result.IsValid = false;
                    result.Errors.Add($"You cannot reuse any of your last {policy.HistoryCount} passwords.");
                    break;
                }
            }
        }

        return result;
    }

    public void RecordPasswordHistory(int userId, string passwordHash)
    {
        _policyRepository.AddPasswordHistory(userId, passwordHash);
    }
}
