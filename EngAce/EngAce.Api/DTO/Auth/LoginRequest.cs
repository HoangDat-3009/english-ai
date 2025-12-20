using System.ComponentModel.DataAnnotations;

namespace EngAce.Api.DTO.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "Email or Username is required")]
    public string EmailOrUsername { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}


