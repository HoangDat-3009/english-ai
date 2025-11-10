namespace EngAce.Api.DTO
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public string Role { get; set; } = "user";
        public string Status { get; set; } = "active";
        public bool EmailVerified { get; set; }
    }
}
