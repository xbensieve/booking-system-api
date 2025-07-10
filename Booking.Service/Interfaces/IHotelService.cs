using Booking.Service.Models;

namespace Booking.Service.Interfaces
{
    public interface IHotelService
    {
        Task<ApiResponse<object>> CreateHotelAsync(HotelRequest request);
        Task<ApiResponse<HotelResponse>> GetHotelByIdAsync(int id);
    }
}
