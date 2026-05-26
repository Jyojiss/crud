using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserCrudApi.Common;
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
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(
                    StatusCodes.Status403Forbidden,
                    "No tienes permisos para consultar este usuario."
                )
            );
        }

        var user = await _context.Users.FindAsync(id);

        if (user is null)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound,
                "Usuario no encontrado."
            ));
        }

        return Ok(ToUserResponse(user));
    }

    [HttpPost]
[Authorize(Roles = "admin")]
public async Task<ActionResult<UserResponse>> CreateUser(CreateUserRequest request)
{
    var currentUserId = GetCurrentUserId();

    var email = request.Email.Trim().ToLower();

    var emailExists = await _context.Users.AnyAsync(u => u.Email == email);

    if (emailExists)
    {
        return Conflict(ApiResponse<object>.Fail(
            StatusCodes.Status409Conflict,
            "El email ya está registrado."
        ));
    }

    if (request.Role != "admin" && request.Role != "user")
    {
        return BadRequest(ApiResponse<object>.Fail(
            StatusCodes.Status400BadRequest,
            "El rol debe ser admin o user."
        ));
    }

    var user = new User
    {
        Email = email,
        Name = request.Name.Trim(),
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        Role = request.Role,
        IsActive = true,
        CreatedBy = currentUserId,
        UpdatedBy = null
    };

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ToUserResponse(user));
}

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(Guid id, UpdateUserDto request)
    {
        var currentUserId = GetCurrentUserId();
        var isAdmin = IsAdmin();

        if (currentUserId is null)
        {
            return Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Usuario no autenticado."
            ));
        }

        var isOwner = currentUserId == id;

        if (!isAdmin && !isOwner)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(
                    StatusCodes.Status403Forbidden,
                    "No tienes permisos para editar este usuario."
                )
            );
        }

        var user = await _context.Users.FindAsync(id);

        if (user is null)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound,
                "Usuario no encontrado."
            ));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest,
                "El nombre es obligatorio."
            ));
        }

        user.Name = request.Name.Trim();

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim().ToLower();

            if (email != user.Email)
            {
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == email && u.Id != id);

                if (emailExists)
                {
                    return Conflict(ApiResponse<object>.Fail(
                        StatusCodes.Status409Conflict,
                        "El email ya está registrado."
                    ));
                }

                user.Email = email;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            if (request.Password.Length < 8)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "La contraseña debe tener mínimo 8 caracteres."
                ));
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        if (isAdmin)
        {
            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                var role = request.Role.Trim().ToLower();

                if (role != "admin" && role != "user")
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "El rol debe ser admin o user."
                    ));
                }

                user.Role = role;
            }

            if (request.IsActive.HasValue)
            {
                user.IsActive = request.IsActive.Value;
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(request.Role) && request.Role != user.Role)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status403Forbidden,
                        "No tienes permisos para modificar el rol."
                    )
                );
            }

            if (request.IsActive.HasValue && request.IsActive.Value != user.IsActive)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status403Forbidden,
                        "No tienes permisos para modificar el estado del usuario."
                    )
                );
            }
        }

        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = currentUserId;

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
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound,
                "Usuario no encontrado."
            ));
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
            return Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Usuario no autenticado."
            ));
        }

        var user = await _context.Users.FindAsync(currentUserId);

        if (user is null)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound,
                "Usuario no encontrado."
            ));
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
        UpdatedAt = user.UpdatedAt,
        CreatedBy = user.CreatedBy,
        UpdatedBy = user.UpdatedBy
    };
}
}