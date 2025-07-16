using Booking.Application.DTOs.Auth;
using Booking.Application.DTOs.Common;

namespace Booking.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<object>> RegisterAsync(RegisterModel model);
        Task<ApiResponse<object>> GetUserByIdAsync(string uid);
    }
}
