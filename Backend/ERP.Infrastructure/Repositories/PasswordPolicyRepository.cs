using ERP.Application.Interfaces.Repositories;
using ERP.Domain.Common;
using ERP.Domain.Entities;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;

public class PasswordPolicyRepository : IPasswordPolicyRepository
{
    private readonly ApplicationDbContext _context;

    public PasswordPolicyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public PasswordPolicy GetPolicy()
    {
        var policy = _context.PasswordPolicies.FirstOrDefault();
        if (policy == null)
        {
            policy = new PasswordPolicy
            {
                MinLength = 8,
                RequireUppercase = true,
                RequireLowercase = true,
                RequireDigit = true,
                RequireSpecialChar = true,
                ExpiryDays = 90,
                HistoryCount = 5,
                MaxFailedAttempts = 5,
                LockoutDurationMinutes = 15,
                UpdatedAt = TimeHelper.Now
            };
            _context.PasswordPolicies.Add(policy);
            _context.SaveChanges();
        }
        return policy;
    }

    public void UpdatePolicy(PasswordPolicy policy)
    {
        _context.PasswordPolicies.Update(policy);
        _context.SaveChanges();
    }

    public void AddPasswordHistory(int userId, string passwordHash)
    {
        _context.PasswordHistories.Add(new PasswordHistory
        {
            UserId = userId,
            PasswordHash = passwordHash,
            CreatedAt = TimeHelper.Now
        });
        _context.SaveChanges();
    }

    public List<PasswordHistory> GetRecentPasswordHistory(int userId, int count)
    {
        return _context.PasswordHistories
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAt)
            .Take(count)
            .ToList();
    }
}
