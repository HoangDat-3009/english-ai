using System.Text.Json.Serialization;

namespace EngAce.Api.DTO
{
    public class AuthResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        
        [JsonPropertyName("token")]
        public string? Token { get; set; }
        
        [JsonPropertyName("user")]
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        [JsonPropertyName("userId")]
        public int UserId { get; set; }
        
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        
        [JsonPropertyName("username")]
        public string? Username { get; set; }
        
        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }
        
        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }
        
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";
        
        [JsonPropertyName("status")]
        public string Status { get; set; } = "active";
        
        [JsonPropertyName("emailVerified")]
        public bool EmailVerified { get; set; }
    }
}
