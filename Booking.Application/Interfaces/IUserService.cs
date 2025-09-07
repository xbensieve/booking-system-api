using Booking.Application.DTOs.Auth;
using Booking.Application.DTOs.Common;

namespace Booking.Application.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<List<UserResponse>>> GetAllUsersAsync(int page, int pageSize);
        Task<ApiResponse<UserResponse>> UpdateUserAsync(Guid userId, DTOs.User.UpdateUserRequest request);
        Task<ApiResponse<object>> DeleteUserAsync(Guid userId);
    }
}
