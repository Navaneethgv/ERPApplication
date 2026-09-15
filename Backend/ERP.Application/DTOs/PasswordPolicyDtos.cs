using System.ComponentModel.DataAnnotations;

namespace ERP.Application.DTOs;

public class PasswordPolicyDto
{
    public int Id { get; set; }
    public int MinLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialChar { get; set; } = true;
    public int ExpiryDays { get; set; } = 90;
    public int HistoryCount { get; set; } = 5;
    public int MaxFailedAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
    public DateTime UpdatedAt { get; set; }
}

public class UpdatePasswordPolicyDto
{
    [Range(6, 128, ErrorMessage = "Minimum password length must be between 6 and 128 characters.")]
    public int MinLength { get; set; } = 8;

    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialChar { get; set; } = true;

    [Range(0, 3650, ErrorMessage = "Expiry days must be between 0 (no expiry) and 3650.")]
    public int ExpiryDays { get; set; } = 90;

    [Range(0, 24, ErrorMessage = "History count must be between 0 and 24.")]
    public int HistoryCount { get; set; } = 5;

    [Range(1, 100, ErrorMessage = "Max failed attempts must be between 1 and 100.")]
    public int MaxFailedAttempts { get; set; } = 5;

    [Range(1, 1440, ErrorMessage = "Lockout duration must be between 1 and 1440 minutes.")]
    public int LockoutDurationMinutes { get; set; } = 15;
}

public class PasswordValidationResultDto
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
