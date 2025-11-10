using System.ComponentModel.DataAnnotations;

namespace EngAce.Api.DTO
{
    public class OAuthLoginRequest
    {
        [Required]
        public string Provider { get; set; } = string.Empty; // "google" or "facebook"

        [Required]
        public string ProviderId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public string? Avatar { get; set; }
    }
}
