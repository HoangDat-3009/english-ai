using EngAce.Api.DTO.Auth;

namespace EngAce.Api.Services.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> OAuthLoginAsync(OAuthLoginRequest request);
    Task<UserDto?> GetUserByIdAsync(int userId);
}

