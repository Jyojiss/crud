using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserCrudApi.Data;
using UserCrudApi.DTOs;
using UserCrudApi.Models;
using UserCrudApi.Services;

namespace UserCrudApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email);

        if (emailExists)
        {
            return Conflict(new { message = "El email ya está registrado." });
        }

        var user = new User
        {
            Email = request.Email.Trim().ToLower(),
            Name = request.Name.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "user",
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            AccessToken = token,
            User = ToUserResponse(user)
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLower();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new { message = "El usuario está inactivo." });
        }

        var validPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!validPassword)
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        var token = _tokenService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            AccessToken = token,
            User = ToUserResponse(user)
        });
    }

    private static UserResponse ToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
