using SystemZarzadzaniaBibliotekaFirmy.DTOs;

namespace SystemZarzadzaniaBibliotekaFirmy.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    Task<bool> HasAnyUsersAsync();
}

