using BCrypt.Net;
using EngAce.Api.DTO;
using EngAce.Api.Repositories;
using Entities;
using System;
using System.Threading.Tasks;

namespace EngAce.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public AuthService(IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Check if email already exists
                if (await _userRepository.EmailExistsAsync(request.Email))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Email already exists"
                    };
                }

                // Check if username already exists
                if (!string.IsNullOrWhiteSpace(request.Username) && 
                    await _userRepository.UsernameExistsAsync(request.Username))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Username already exists"
                    };
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

                // Create user
                var user = new User
                {
                    Email = request.Email.ToLower().Trim(),
                    Username = request.Username?.Trim(),
                    FullName = request.FullName?.Trim(),
                    PasswordHash = passwordHash,
                    Phone = request.Phone,
                    Role = "user",
                    Status = "active",
                    EmailVerified = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdUser = await _userRepository.CreateAsync(user);

                // Generate JWT token
                var token = _jwtService.GenerateToken(createdUser.UserID, createdUser.Email, createdUser.Role);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Registration successful",
                    Token = token,
                    User = new UserDto
                    {
                        UserId = createdUser.UserID,
                        Email = createdUser.Email,
                        Username = createdUser.Username,
                        FullName = createdUser.FullName,
                        Avatar = createdUser.Avatar,
                        Role = createdUser.Role,
                        Status = createdUser.Status,
                        EmailVerified = createdUser.EmailVerified
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Registration failed: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                // Find user by email or username
                User? user = null;
                
                if (request.EmailOrUsername.Contains("@"))
                {
                    user = await _userRepository.GetByEmailAsync(request.EmailOrUsername.ToLower().Trim());
                }
                else
                {
                    user = await _userRepository.GetByUsernameAsync(request.EmailOrUsername.Trim());
                }

                if (user == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email/username or password"
                    };
                }

                // Check if user has password (not OAuth-only user)
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "This account uses social login. Please login with Google or Facebook."
                    };
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email/username or password"
                    };
                }

                // Check account status
                if (user.Status != "active")
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = $"Account is {user.Status}. Please contact support."
                    };
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                // Generate JWT token
                var token = _jwtService.GenerateToken(user.UserID, user.Email, user.Role, request.RememberMe);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    User = new UserDto
                    {
                        UserId = user.UserID,
                        Email = user.Email,
                        Username = user.Username,
                        FullName = user.FullName,
                        Avatar = user.Avatar,
                        Role = user.Role,
                        Status = user.Status,
                        EmailVerified = user.EmailVerified
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Login failed: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> OAuthLoginAsync(OAuthLoginRequest request)
        {
            try
            {
                User? user = null;

                // Find existing user by provider ID
                if (request.Provider.ToLower() == "google")
                {
                    user = await _userRepository.GetByGoogleIdAsync(request.ProviderId);
                }
                else if (request.Provider.ToLower() == "facebook")
                {
                    user = await _userRepository.GetByFacebookIdAsync(request.ProviderId);
                }

                // If not found by provider ID, try to find by email
                if (user == null)
                {
                    user = await _userRepository.GetByEmailAsync(request.Email.ToLower().Trim());
                    
                    if (user != null)
                    {
                        // Link OAuth account to existing user
                        if (request.Provider.ToLower() == "google")
                        {
                            user.GoogleID = request.ProviderId;
                        }
                        else if (request.Provider.ToLower() == "facebook")
                        {
                            user.FacebookID = request.ProviderId;
                        }
                        
                        user.Avatar = user.Avatar ?? request.Avatar;
                        user.FullName = user.FullName ?? request.FullName;
                        user.LastLoginAt = DateTime.UtcNow;
                        
                        await _userRepository.UpdateAsync(user);
                    }
                }

                // Create new user if not exists
                if (user == null)
                {
                    user = new User
                    {
                        Email = request.Email.ToLower().Trim(),
                        FullName = request.FullName?.Trim(),
                        Avatar = request.Avatar,
                        Role = "user",
                        Status = "active",
                        EmailVerified = true, // OAuth emails are pre-verified
                        PasswordHash = null, // No password for OAuth users
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        LastLoginAt = DateTime.UtcNow
                    };

                    if (request.Provider.ToLower() == "google")
                    {
                        user.GoogleID = request.ProviderId;
                    }
                    else if (request.Provider.ToLower() == "facebook")
                    {
                        user.FacebookID = request.ProviderId;
                    }

                    user = await _userRepository.CreateAsync(user);
                }
                else
                {
                    // Update last login for existing user
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user);
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(user.UserID, user.Email, user.Role);

                return new AuthResponse
                {
                    Success = true,
                    Message = "OAuth login successful",
                    Token = token,
                    User = new UserDto
                    {
                        UserId = user.UserID,
                        Email = user.Email,
                        Username = user.Username,
                        FullName = user.FullName,
                        Avatar = user.Avatar,
                        Role = user.Role,
                        Status = user.Status,
                        EmailVerified = user.EmailVerified
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"OAuth login failed: {ex.Message}"
                };
            }
        }
    }
}
