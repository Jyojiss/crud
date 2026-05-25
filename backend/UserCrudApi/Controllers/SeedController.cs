using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserCrudApi.Data;
using UserCrudApi.Models;

namespace UserCrudApi.Controllers;

[ApiController]
[Route("api/seed")]
public class SeedController : ControllerBase
{
    private readonly AppDbContext _context;

    public SeedController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult> CreateSeedUsers()
    {
        if (!await _context.Users.AnyAsync(u => u.Email == "admin@demo.com"))
        {
            _context.Users.Add(new User
            {
                Email = "admin@demo.com",
                Name = "Admin Demo",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "admin",
                IsActive = true
            });
        }

        if (!await _context.Users.AnyAsync(u => u.Email == "user@demo.com"))
        {
            _context.Users.Add(new User
            {
                Email = "user@demo.com",
                Name = "User Demo",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                Role = "user",
                IsActive = true
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Usuarios de prueba creados correctamente." });
    }
}