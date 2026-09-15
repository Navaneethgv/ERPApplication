using ERP.Application.DTOs;

namespace ERP.Application.Interfaces.Services;

public interface IPasswordPolicyService
{
    PasswordPolicyDto GetPolicy();
    PasswordPolicyDto UpdatePolicy(UpdatePasswordPolicyDto dto);
    PasswordValidationResultDto ValidatePassword(string password, int? userId = null);
    void RecordPasswordHistory(int userId, string passwordHash);
}
