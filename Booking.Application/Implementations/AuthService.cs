using Booking.Application.DTOs.Auth;
using Booking.Application.DTOs.Common;
using Booking.Application.Interfaces;
using Booking.Domain.Entities;
using Booking.Domain.Interfaces;
using Booking.Infrastructure.ExternalService.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Booking.Application.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly IBackgroundTaskQueue _taskQueue;
        public AuthService(IUnitOfWork unitOfWork, IConfiguration config, IBackgroundTaskQueue taskQueue)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _taskQueue = taskQueue;
        }
        public async Task<ApiResponse<object>> GetUserByIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                return ApiResponse<object>.Fail("User ID is required.");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
                return ApiResponse<object>.Fail("User not found.", null);

            return ApiResponse<object>.Ok(user, "User retrieved successfully.");
        }
        public async Task<ApiResponse<object>> LoginAsync(LoginModel model)
        {
            if (model == null)
                return ApiResponse<object>.Fail("Invalid login request.");

            var user = await _unitOfWork.Users.Query()
                .FirstOrDefaultAsync(u => u.Email == model.Email && !u.IsDeleted);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                return ApiResponse<object>.Fail("Invalid email or password.");

            if (!user.IsActive)
                return ApiResponse<object>.Fail("User account is inactive.");

            if (!user.IsVerified)
                return ApiResponse<object>.Fail("User account is not verified.");

            var accessToken = GenerateJwtToken(user);

            var existingToken = await _unitOfWork.RefreshTokens.Query().FirstOrDefaultAsync(rt => rt.UserId == user.UserId && rt.Expires > DateTime.UtcNow);

            string rawRefreshToken;

            if (existingToken != null)
            {
                rawRefreshToken = "[cannot return stored hash]";

                _unitOfWork.RefreshTokens.Delete(existingToken);

                rawRefreshToken = Guid.NewGuid().ToString("N");
                var newRefreshToken = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    TokenHash = BCrypt.Net.BCrypt.HashPassword(rawRefreshToken),
                    Expires = DateTime.UtcNow.AddDays(
                        int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "7")
                    ),
                    UserId = user.UserId
                };
                await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
            }
            else
            {
                rawRefreshToken = Guid.NewGuid().ToString("N");
                var refreshToken = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    TokenHash = BCrypt.Net.BCrypt.HashPassword(rawRefreshToken),
                    Expires = DateTime.UtcNow.AddDays(
                        int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "7")
                    ),
                    UserId = user.UserId
                };
                await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            }

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<object>.Ok(new
            {
                Message = "Login successful",
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                User = new
                {
                    user.UserId,
                    user.Email,
                    user.Name,
                    user.PhoneNumber,
                    user.AvatarUrl
                }
            });
        }
        public async Task<ApiResponse<object>> RefreshTokenAsync(string expiredAccessToken, string refreshTokenRaw)
        {
            if (string.IsNullOrWhiteSpace(expiredAccessToken) || string.IsNullOrWhiteSpace(refreshTokenRaw))
                return ApiResponse<object>.Fail("Invalid token request.");

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(expiredAccessToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config["Jwt:Key"])
                )
            }, out _);

            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return ApiResponse<object>.Fail("Invalid user.");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
                return ApiResponse<object>.Fail("User not found.");

            var storedToken = await _unitOfWork.RefreshTokens.Query()
                .Where(rt => rt.UserId == userId && rt.Expires > DateTime.UtcNow)
                .OrderByDescending(rt => rt.Expires)
                .FirstOrDefaultAsync();

            if (storedToken == null || !BCrypt.Net.BCrypt.Verify(refreshTokenRaw, storedToken.TokenHash))
                return ApiResponse<object>.Fail("Invalid or expired refresh token.");

            _unitOfWork.RefreshTokens.Delete(storedToken);

            var newAccessToken = GenerateJwtToken(user);
            var newRawRefreshToken = Guid.NewGuid().ToString("N");

            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                TokenHash = BCrypt.Net.BCrypt.HashPassword(newRawRefreshToken),
                Expires = DateTime.UtcNow.AddDays(
                    int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "7")
                ),
                UserId = user.UserId
            };

            await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<object>.Ok(new
            {
                Message = "Token refreshed successfully",
                AccessToken = newAccessToken,
                RefreshToken = newRawRefreshToken,
                User = new
                {
                    user.UserId,
                    user.Email,
                    user.Name,
                    user.PhoneNumber,
                    user.AvatarUrl
                }
            });
        }
        public async Task<ApiResponse<object>> RegisterAsync(RegisterModel model)
        {
            if (model == null)
                return ApiResponse<object>.Fail("Invalid user data.");

            var existingUser = await _unitOfWork.Users.Query()
                .FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
                return ApiResponse<object>.Fail("Email already exists.");

            var existingPhone = await _unitOfWork.Users.Query()
                .FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);
            if (existingPhone != null)
                return ApiResponse<object>.Fail("Phone number already exists.");

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = model.Email,
                Name = model.FullName,
                PhoneNumber = model.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                CreatedAt = DateTime.UtcNow,
            };

            try
            {
                await _unitOfWork.Users.AddAsync(user);
                int result = await _unitOfWork.SaveChangesAsync();

                _taskQueue.QueueBackgroundWorkItem(async serviceProvider =>
                {
                    var scopedUnitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
                    var emailService = serviceProvider.GetRequiredService<IEmailService>();

                    try
                    {
                        var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

                        var userOtp = new UserOtp
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.UserId,
                            Code = BCrypt.Net.BCrypt.HashPassword(otpCode),
                            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                            IsUsed = false,
                        };

                        await scopedUnitOfWork.UserOtps.AddAsync(userOtp);
                        await scopedUnitOfWork.SaveChangesAsync();

                        string subject = "Your OTP Code";
                        string htmlBody = $@"
                                        <h2>Verification Required</h2>
                                        <p>Your one-time password (OTP) is:</p>
                                        <h1 style='color:blue;'>{otpCode}</h1>
                                        <p>This code will expire in 10 minutes.</p>
                                        <br/>
                                        <p>- XBensieve Security Team</p>";

                        await emailService.SendEmailAsync(user.Email, subject, htmlBody);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to process OTP request.\n{ex.Message}", ex);
                    }
                });



                if (result > 0)
                {
                    return ApiResponse<object>.Ok(new
                    {
                        Message = "User registered successfully",
                        User = new
                        {
                            user.UserId,
                            user.Email,
                            user.Name,
                            user.PhoneNumber,
                            user.AvatarUrl
                        }
                    });
                }
                else
                {
                    return ApiResponse<object>.Fail("Failed to register user.");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail($"An error occurred while registering the user: {ex.Message}");
            }
        }
        public async Task<ApiResponse<object>> LogoutAsync(string refreshTokenRaw)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenRaw))
                return ApiResponse<object>.Fail("Invalid refresh token.");

            var tokens = await _unitOfWork.RefreshTokens.Query()
                .Where(rt => rt.Expires > DateTime.UtcNow)
                .ToListAsync();

            var storedToken = tokens.FirstOrDefault(rt =>
                BCrypt.Net.BCrypt.Verify(refreshTokenRaw, rt.TokenHash));

            if (storedToken == null)
                return ApiResponse<object>.Fail("Refresh token not found or already invalidated.");

            _unitOfWork.RefreshTokens.Delete(storedToken);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<object>.Ok(new { Message = "Logout successful" });
        }
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("name", user.Name ?? string.Empty),
                new Claim("phone", user.PhoneNumber ?? string.Empty)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(
                    int.Parse(_config["Jwt:AccessTokenExpirationMinutes"] ?? "15")
                ),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public async Task<ApiResponse<object>> VerifyEmailByOtpAsync(string email, string inputOtp)
        {
            var user = await _unitOfWork.Users.Query()
           .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return ApiResponse<object>.Fail("User not found");

            if (user.IsVerified)
                return ApiResponse<object>.Fail("Email already verified");

            var otp = await _unitOfWork.UserOtps.Query()
                .Where(o => o.UserId == user.UserId && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.ExpiresAt)
                .FirstOrDefaultAsync();

            if (otp == null || !BCrypt.Net.BCrypt.Verify(inputOtp, otp.Code))
                return ApiResponse<object>.Fail("Invalid or expired OTP");

            otp.IsUsed = true;
            user.IsVerified = true;

            _unitOfWork.UserOtps.Update(otp);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            var accessToken = GenerateJwtToken(user);
            var existingToken = await _unitOfWork.RefreshTokens.Query().FirstOrDefaultAsync(rt => rt.UserId == user.UserId && rt.Expires > DateTime.UtcNow);

            string rawRefreshToken;

            if (existingToken != null)
            {
                rawRefreshToken = "[cannot return stored hash]";

                _unitOfWork.RefreshTokens.Delete(existingToken);

                rawRefreshToken = Guid.NewGuid().ToString("N");
                var newRefreshToken = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    TokenHash = BCrypt.Net.BCrypt.HashPassword(rawRefreshToken),
                    Expires = DateTime.UtcNow.AddDays(
                        int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "7")
                    ),
                    UserId = user.UserId
                };
                await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
            }
            else
            {
                rawRefreshToken = Guid.NewGuid().ToString("N");
                var refreshToken = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    TokenHash = BCrypt.Net.BCrypt.HashPassword(rawRefreshToken),
                    Expires = DateTime.UtcNow.AddDays(
                        int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "7")
                    ),
                    UserId = user.UserId
                };
                await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            }

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<object>.Ok(new
            {
                Message = "Email verified successfully",
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                User = new
                {
                    user.UserId,
                    user.Email,
                    user.Name,
                    user.PhoneNumber,
                    user.AvatarUrl
                }
            });
        }
        public async Task<ApiResponse<object>> ResendOtpByEmailAsync(string email)
        {
            var user = await _unitOfWork.Users.Query()
           .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

            if (user == null)
                return ApiResponse<object>.Fail("User not found");

            if (user.IsVerified)
                return ApiResponse<object>.Fail("Email already verified");

            var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            var newOtp = new UserOtp
            {
                Id = Guid.NewGuid(),
                UserId = user.UserId,
                Code = BCrypt.Net.BCrypt.HashPassword(otpCode),
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            await _unitOfWork.UserOtps.AddAsync(newOtp);
            await _unitOfWork.SaveChangesAsync();

            _taskQueue.QueueBackgroundWorkItem(async serviceProvider =>
            {
                var emailService = serviceProvider.GetRequiredService<IEmailService>();
                try
                {
                    string subject = "Your OTP Code";
                    string htmlBody = $@"
                                         <h2>Verification Required</h2>
                                         <p>Your one-time password (OTP) is:</p>
                                         <h1 style='color:blue;'>{otpCode}</h1>
                                         <p>This code will expire in 10 minutes.</p>
                                         <br/>
                                         <p>- XBensieve Security Team</p>";

                    await emailService.SendEmailAsync(user.Email, subject, htmlBody);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to send OTP email.\n{ex.Message}", ex);
                }
            });
            return ApiResponse<object>.Ok(new { Message = "OTP resent successfully" });
        }
        public async Task<ApiResponse<object>> ForgotPasswordAsync(string email)
        {
            var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

            if (user == null)
                return ApiResponse<object>.Ok(new { Message = "If this email exists, an OTP has been sent." });

            var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var otp = new UserOtp
            {
                Id = Guid.NewGuid(),
                UserId = user.UserId,
                Code = BCrypt.Net.BCrypt.HashPassword(otpCode),
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            await _unitOfWork.UserOtps.AddAsync(otp);
            await _unitOfWork.SaveChangesAsync();

            _taskQueue.QueueBackgroundWorkItem(async serviceProvider =>
            {
                var emailService = serviceProvider.GetRequiredService<IEmailService>();
                try
                {
                    string subject = "Your Password Reset OTP Code";
                    string htmlBody = $@"
                                        <h2>Password Reset Request</h2>
                                        <p>Your one-time password (OTP) for resetting your password is:</p>
                                        <h1 style='color:blue;'>{otpCode}</h1>
                                        <p>This code will expire in 10 minutes.</p>
                                        <br/>
                                        <p>- XBensieve Security Team</p>";
                    await emailService.SendEmailAsync(user.Email, subject, htmlBody);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to send password reset OTP email.\n{ex.Message}", ex);
                }
            });

            return ApiResponse<object>.Ok(new { Message = "If this email exists, an OTP has been sent." });
        }
        public async Task<ApiResponse<object>> ResetPasswordAsync(Guid userId, string newPassword)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return ApiResponse<object>.Fail("User not found");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<object>.Ok(new { Message = "Password has been reset successfully" });
        }
        public async Task<ApiResponse<object>> VerifyResetOtpAsync(string email, string inputOtp)
        {
            var user = await _unitOfWork.Users.Query().FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return ApiResponse<object>.Fail("Invalid email or OTP");

            var otp = await _unitOfWork.UserOtps.Query()
                .Where(o => o.UserId == user.UserId && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.ExpiresAt)
                .FirstOrDefaultAsync();

            if (otp == null || !BCrypt.Net.BCrypt.Verify(inputOtp, otp.Code))
                return ApiResponse<object>.Fail("Invalid or expired OTP");

            otp.IsUsed = true;
            _unitOfWork.UserOtps.Update(otp);
            await _unitOfWork.SaveChangesAsync();

            var resetToken = GenerateJwtToken(user);

            return ApiResponse<object>.Ok(new
            {
                Message = "OTP verified successfully",
                ResetToken = resetToken
            });
        }
        private int DecodeResetToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out _);

            return int.Parse(principal.FindFirst("UserId")!.Value);
        }
    }
}
