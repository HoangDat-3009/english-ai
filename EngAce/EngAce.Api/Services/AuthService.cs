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
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, IJwtService jwtService, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
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
                    Role = "user", // Default role
                    Status = "active",
                    AccountType = "free",
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
                        Role = "user",
                        Status = createdUser.Status,
                        EmailVerified = true
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
                _logger.LogInformation("🔐 Login attempt for: {EmailOrUsername}", request.EmailOrUsername);
                
                // Find user by email or username
                User? user = null;
                
                if (request.EmailOrUsername.Contains("@"))
                {
                    _logger.LogInformation("📧 Searching by email...");
                    user = await _userRepository.GetByEmailAsync(request.EmailOrUsername.ToLower().Trim());
                }
                else
                {
                    _logger.LogInformation("👤 Searching by username...");
                    user = await _userRepository.GetByUsernameAsync(request.EmailOrUsername.Trim());
                }

                if (user == null)
                {
                    _logger.LogWarning("❌ User not found: {EmailOrUsername}", request.EmailOrUsername);
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email/username or password"
                    };
                }

                // Check if user has password (not OAuth-only user)
                _logger.LogInformation($"🔍 Password Hash Check - IsNull: {user.PasswordHash == null}, IsEmpty: {user.PasswordHash == string.Empty}, Length: {user.PasswordHash?.Length ?? 0}");
                
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    _logger.LogWarning("⚠️ OAuth-only account attempted password login: {Email}", user.Email);
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "This account uses social login. Please login with Google or Facebook."
                    };
                }

                // Verify password
                _logger.LogInformation("🔑 Verifying password...");
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("❌ Invalid password for: {EmailOrUsername}", request.EmailOrUsername);
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email/username or password"
                    };
                }
                _logger.LogInformation("✅ Password verified successfully");

                // Check account status
                if (user.Status != "active")
                {
                    _logger.LogWarning("⚠️ Inactive account login attempt: {Email} (Status: {Status})", user.Email, user.Status);
                    return new AuthResponse
                    {
                        Success = false,
                        Message = $"Account is {user.Status}. Please contact support."
                    };
                }

                // Update last login
                _logger.LogInformation("📅 Updating last login time...");
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                // Generate JWT token
                _logger.LogInformation("🎟️ Generating JWT token...");
                var token = _jwtService.GenerateToken(user.UserID, user.Email, user.Role, request.RememberMe);

                _logger.LogInformation("✅ Login successful for: {Email} (UserID: {UserId})", user.Email, user.UserID);
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
                        Role = "user",
                        Status = user.Status,
                        EmailVerified = true
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Login error for: {EmailOrUsername}", request.EmailOrUsername);
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
                            user.GoogleId = request.ProviderId;
                        }
                        else if (request.Provider.ToLower() == "facebook")
                        {
                            user.FacebookId = request.ProviderId;
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
                        Role = "user", // Default role
                        Status = "active",
                        AccountType = "free",
                        PasswordHash = null, // No password for OAuth users
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        LastLoginAt = DateTime.UtcNow
                    };

                    if (request.Provider.ToLower() == "google")
                    {
                        user.GoogleId = request.ProviderId;
                    }
                    else if (request.Provider.ToLower() == "facebook")
                    {
                        user.FacebookId = request.ProviderId;
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
                        Role = "user",
                        Status = user.Status,
                        EmailVerified = true
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
