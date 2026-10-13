using ExpenseTracker.DTOs.Auth;
using ExpenseTracker.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;

    public AuthController(IAuthService authService)
    {
        this.authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        AuthResponseDto? result =
            await authService.RegisterAsync(dto);

        if (result == null)
        {
            return BadRequest(new
            {
                message = "Registration failed."
            });
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        AuthResponseDto? result =
            await authService.LoginAsync(dto);

        if (result == null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        return Ok(result);
    }
}