using Booking.Application.DTOs.Common;
using Booking.Application.DTOs.Reservation;

namespace Booking.Application.Interfaces
{
    public interface IAdminService
    {
        Task<ApiResponse<List<ReservationResponse>>> GetReservationsByDateAsync(DateTime today, int page, int pageSize);
    }
}
