using ExpenseTracker.DTOs.Auth;

namespace ExpenseTracker.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto dto);

    Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto);
}