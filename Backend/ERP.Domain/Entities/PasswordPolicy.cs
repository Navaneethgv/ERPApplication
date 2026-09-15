using ERP.Domain.Common;

namespace ERP.Domain.Entities;

public class PasswordPolicy
{
    public int Id { get; set; }
    public int MinLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialChar { get; set; } = true;
    public int ExpiryDays { get; set; } = 90; // 0 means no expiry
    public int HistoryCount { get; set; } = 5; // Number of previous passwords to prevent reusing
    public int MaxFailedAttempts { get; set; } = 5; // Lockout threshold
    public int LockoutDurationMinutes { get; set; } = 15; // Lockout duration in minutes
    public DateTime UpdatedAt { get; set; } = TimeHelper.Now;
}
