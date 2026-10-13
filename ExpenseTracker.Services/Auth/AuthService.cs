using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExpenseTracker.Data.Models.Entities;
using ExpenseTracker.DTOs.Auth;
using ExpenseTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseTracker.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IConfiguration configuration;

    public AuthService(UserManager<ApplicationUser> userManager,IConfiguration configuration)
    {
        this.userManager = userManager;
        this.configuration = configuration;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto dto)
    {
        ApplicationUser? existingUser =
            await userManager.FindByEmailAsync(dto.Email);

        if (existingUser != null)
        {
            return null;
        }

        ApplicationUser user = new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            CreatedOn = DateTime.UtcNow
        };

        IdentityResult result =
            await userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return null;
        }

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto)
    {
        ApplicationUser? user =
            await userManager.FindByEmailAsync(dto.Email);

        if (user == null)
        {
            return null;
        }

        bool isPasswordValid =
            await userManager.CheckPasswordAsync(user,dto.Password);

        if (!isPasswordValid)
        {
            return null;
        }

        return GenerateAuthResponse(user);
    }

    private AuthResponseDto GenerateAuthResponse(ApplicationUser user)
    {
        string jwtKey =
            configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key was not found.");

        string jwtIssuer =
            configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer was not found.");

        string jwtAudience =
            configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT audience was not found.");

        DateTime expiresAt =
            DateTime.UtcNow.AddHours(2);

        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.NameIdentifier,user.Id),

            new Claim(ClaimTypes.Email,user.Email!),

            new Claim(ClaimTypes.Name,$"{user.FirstName} {user.LastName}")
        };

        SymmetricSecurityKey securityKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        SigningCredentials credentials =
            new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token =
            new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

        string tokenValue =
            new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponseDto
        {
            Token = tokenValue,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }
}