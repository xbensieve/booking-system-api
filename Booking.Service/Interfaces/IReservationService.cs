using Booking.Service.Models;

namespace Booking.Service.Interfaces
{
    public interface IReservationService
    {
        Task<ApiResponse<object>> CreateReservationAsync(string UserId, ReservationRequest request);
        Task CancelReservationAsync(int reservationId);
        Task<ApiResponse<ReservationResponse>> GetReservationByIdAsync(int reservationId);
        Task<ApiResponse<List<ReservationResponse>>> GetReservationsByUserIdAsync(string userId, int page, int pageSize);
    }
}
