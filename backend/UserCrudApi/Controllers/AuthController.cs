using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserCrudApi.Common;
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
        var email = request.Email.Trim().ToLower();

        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == email);

        if (emailExists)
        {
            return Conflict(ApiResponse<object>.Fail(
                StatusCodes.Status409Conflict,
                "El email ya está registrado."
            ));
        }

        var user = new User
        {
            Email = email,
            Name = request.Name.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "user",
            IsActive = true,
            CreatedBy = null,
            UpdatedBy = null
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var accessToken = _tokenService.GenerateToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
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
            return Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Credenciales inválidas."
            ));
        }

        if (!user.IsActive)
        {
            return Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "El usuario está inactivo."
            ));
        }

        var validPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!validPassword)
        {
            return Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Credenciales inválidas."
            ));
        }

        var accessToken = _tokenService.GenerateToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = ToUserResponse(user)
        });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request)
    {
        var refreshTokenHash = _tokenService.HashRefreshToken(request.RefreshToken);

        var storedRefreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == refreshTokenHash);

        if (storedRefreshToken is null)
        {
            return Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Refresh token inválido."
            ));
        }

        if (storedRefreshToken.IsRevoked)
        {
            return Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Refresh token revocado."
            ));
        }

        if (storedRefreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            return Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Refresh token expirado."
            ));
        }

        if (!storedRefreshToken.User.IsActive)
        {
            return Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "El usuario está inactivo."
            ));
        }

        storedRefreshToken.IsRevoked = true;
        storedRefreshToken.RevokedAt = DateTime.UtcNow;

        var newAccessToken = _tokenService.GenerateToken(storedRefreshToken.User);
        var newRefreshToken = await CreateRefreshTokenAsync(storedRefreshToken.UserId);

        await _context.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            User = ToUserResponse(storedRefreshToken.User)
        });
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout(RefreshTokenRequest request)
    {
        var refreshTokenHash = _tokenService.HashRefreshToken(request.RefreshToken);

        var storedRefreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == refreshTokenHash);

        if (storedRefreshToken is null)
        {
            return Ok(new { message = "Sesión cerrada correctamente." });
        }

        if (!storedRefreshToken.IsRevoked)
        {
            storedRefreshToken.IsRevoked = true;
            storedRefreshToken.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        return Ok(new { message = "Sesión cerrada correctamente." });
    }

    private async Task<string> CreateRefreshTokenAsync(Guid userId)
    {
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = _tokenService.HashRefreshToken(refreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = userId,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return refreshToken;
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
            UpdatedAt = user.UpdatedAt,
            CreatedBy = user.CreatedBy,
            UpdatedBy = user.UpdatedBy
        };
    }
}