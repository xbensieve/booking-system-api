using Booking.Service.Models;

namespace Booking.Service.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<object>> RegisterAsync(RegisterModel model);
        Task<ApiResponse<object>> GetUserByIdAsync(string uid);
    }
}
