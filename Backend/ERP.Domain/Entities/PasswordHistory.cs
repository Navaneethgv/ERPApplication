using ERP.Domain.Common;

namespace ERP.Domain.Entities;

public class PasswordHistory
{
    public int PasswordHistoryId { get; set; }
    public int UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = TimeHelper.Now;
    public virtual User User { get; set; } = null!;
}
