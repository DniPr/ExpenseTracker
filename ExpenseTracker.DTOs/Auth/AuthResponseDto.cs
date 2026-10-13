namespace ExpenseTracker.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public string UserId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;
}