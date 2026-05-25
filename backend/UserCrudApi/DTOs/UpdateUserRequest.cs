using System.ComponentModel.DataAnnotations;

namespace UserCrudApi.DTOs;

public class UpdateUserRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Role { get; set; }

    public bool? IsActive { get; set; }
}
