using Booking.Application.DTOs.Auth;
using Booking.Application.DTOs.Common;

namespace Booking.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<object>> LoginAsync(LoginModel model);
        Task<ApiResponse<object>> RegisterAsync(RegisterModel model);
        Task<ApiResponse<object>> VerifyEmailByOtpAsync(string email, string inputOtp);
        Task<ApiResponse<object>> ForgotPasswordAsync(string email);
        Task<ApiResponse<object>> ResetPasswordAsync(Guid userId, string newPassword);
        Task<ApiResponse<object>> VerifyResetOtpAsync(string email, string inputOtp);
        Task<ApiResponse<object>> ResendOtpByEmailAsync(string email);
        Task<ApiResponse<object>> GetUserByIdAsync(Guid userId);
        Task<ApiResponse<object>> RefreshTokenAsync(string expiredAccessToken, string refreshTokenRaw);
        Task<ApiResponse<object>> LogoutAsync(string refreshTokenRaw);
    }
}
