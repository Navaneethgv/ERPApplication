using ERP.Domain.Entities;

namespace ERP.Application.Interfaces.Repositories;

public interface IPasswordPolicyRepository
{
    PasswordPolicy GetPolicy();
    void UpdatePolicy(PasswordPolicy policy);
    void AddPasswordHistory(int userId, string passwordHash);
    List<PasswordHistory> GetRecentPasswordHistory(int userId, int count);
}
