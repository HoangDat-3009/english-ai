using EngAce.Api.DTO;
using System.Threading.Tasks;

namespace EngAce.Api.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> OAuthLoginAsync(OAuthLoginRequest request);
    }
}
