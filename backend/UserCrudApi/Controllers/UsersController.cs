using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserCrudApi.Data;
using UserCrudApi.DTOs;
using UserCrudApi.Models;

namespace UserCrudApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<object>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        if (page < 1) page = 1;
        if (size < 1) size = 10;

        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();

            query = query.Where(u =>
                u.Email.ToLower().Contains(normalizedSearch) ||
                u.Name.ToLower().Contains(normalizedSearch));
        }

        var total = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            data = users,
            total,
            page,
            size
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetUser(Guid id)
    {
        if (!IsAdmin() && GetCurrentUserId() != id)
        {
            return Forbid();
        }

        var user = await _context.Users.FindAsync(id);

        if (user is null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        return Ok(ToUserResponse(user));
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<UserResponse>> CreateUser(CreateUserRequest request)
    {
        var email = request.Email.Trim().ToLower();

        var emailExists = await _context.Users.AnyAsync(u => u.Email == email);

        if (emailExists)
        {
            return Conflict(new { message = "El email ya está registrado." });
        }

        if (request.Role != "admin" && request.Role != "user")
        {
            return BadRequest(new { message = "El rol debe ser admin o user." });
        }

        var user = new User
        {
            Email = email,
            Name = request.Name.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ToUserResponse(user));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(Guid id, UpdateUserRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var isAdmin = IsAdmin();

        if (!isAdmin && currentUserId != id)
        {
            return Forbid();
        }

        var user = await _context.Users.FindAsync(id);

        if (user is null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        user.Name = request.Name.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        if (isAdmin)
        {
            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                if (request.Role != "admin" && request.Role != "user")
                {
                    return BadRequest(new { message = "El rol debe ser admin o user." });
                }

                user.Role = request.Role;
            }

            if (request.IsActive.HasValue)
            {
                user.IsActive = request.IsActive.Value;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(ToUserResponse(user));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user is null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserResponse>> GetProfile()
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(currentUserId);

        if (user is null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        return Ok(ToUserResponse(user));
    }

    private Guid? GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(idClaim, out var id))
        {
            return id;
        }

        return null;
    }

    private bool IsAdmin()
    {
        return User.IsInRole("admin");
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
