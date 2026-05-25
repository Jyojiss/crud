using System.ComponentModel.DataAnnotations;

namespace UserCrudApi.DTOs;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
