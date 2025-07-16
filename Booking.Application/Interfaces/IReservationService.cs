using Booking.Application.DTOs.Common;
using Booking.Application.DTOs.Reservation;

namespace Booking.Application.Interfaces
{
    public interface IReservationService
    {
        Task<ApiResponse<object>> CreateReservationAsync(string UserId, ReservationRequest request);
        Task<ApiResponse<object>> CancelReservationAsync(int reservationId);
        Task<ApiResponse<ReservationResponse>> GetReservationByIdAsync(int reservationId);
        Task<ApiResponse<List<ReservationResponse>>> GetReservationsByUserIdAsync(string userId, int page, int pageSize);
        Task<ApiResponse<object>> CheckInReservationAsync(int reservationId, DateTime checkInTime);
        Task<ApiResponse<object>> CheckOutReservationAsync(int reservationId, DateTime checkOutTime);
    }
}
