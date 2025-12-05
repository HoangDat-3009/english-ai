using Entities;
using System.Threading.Tasks;

namespace EngAce.Api.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int userId);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByGoogleIdAsync(string googleId);
        Task<User?> GetByFacebookIdAsync(string facebookId);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
    }
}
