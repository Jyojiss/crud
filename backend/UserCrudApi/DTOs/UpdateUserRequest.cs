using System.ComponentModel.DataAnnotations;

namespace UserCrudApi.DTOs;

public class UpdateUserDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Name { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    public string? Email { get; set; }

    public string? Role { get; set; }

    public bool? IsActive { get; set; }

    public string? Password { get; set; }
}