using Booking.Service.Models;

namespace Booking.Service.Interfaces
{
    public interface IAdminService
    {
        Task<ApiResponse<List<ReservationResponse>>> GetReservationsByDateAsync(DateTime today, int page, int pageSize);
    }
}
