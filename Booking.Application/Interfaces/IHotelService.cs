using Booking.Application.DTOs.Common;
using Booking.Application.DTOs.Hotel;

namespace Booking.Application.Interfaces
{
    public interface IHotelService
    {
        Task<ApiResponse<object>> CreateHotelAsync(HotelRequest request);
        Task<ApiResponse<HotelResponse>> GetHotelByIdAsync(int id);
        Task<ApiResponse<List<HotelResponse>>> GetAllHotelsAsync(int page = 1, int pageSize = 10);
        Task<ApiResponse<object>> UpdateHotelAsync(int id, HotelRequest request);
        Task<ApiResponse<object>> DeleteHotelAsync(int id);
        Task UpdateHotelRatingAsync(int hotelId, double newRating);
        Task UpdateHotelRatingOnReviewEditAsync(int hotelId, int oldRating, int newRating);
        Task UpdateHotelRatingOnReviewDeletedAsync(int hotelId, int deletedRating);
    }
}
