using ERP.Application.DTOs;

namespace ERP.Application.Interfaces.Services;

public interface IAuthService
{
    LoginResponseDto? Login(LoginRequestDto request);
    UserProfileDto? GetUserProfile(int userId);
    bool ChangePassword(int userId, ChangePasswordDto dto);
    UserProfileDto RegisterCustomer(RegisterCustomerDto dto);
}
