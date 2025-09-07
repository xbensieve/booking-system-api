using Booking.Application.DTOs.Common;
using Booking.Application.DTOs.Reservation;

namespace Booking.Application.Interfaces
{
    public interface IAdminService
    {
        Task<ApiResponse<List<ReservationResponse>>> GetReservationsByDateAsync(DateTime today, int page, int pageSize);
        Task<ApiResponse<object>> GetRevenueStatisticsAsync(DateTime startDate, DateTime endDate);
        Task<ApiResponse<object>> GetReservationStatisticsAsync(DateTime startDate, DateTime endDate);
        Task<ApiResponse<object>> GetReviewStatisticsAsync(DateTime startDate, DateTime endDate);
    }
}
