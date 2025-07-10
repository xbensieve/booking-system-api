using Booking.Service.Models;

namespace Booking.Service.Interfaces
{
    public interface IHotelImageService
    {
        Task<ApiResponse<object>> AdddHotelImagesAsync(int hotelId, List<HotelImageRequest> images);
        Task<ApiResponse<HotelImageResponse>> GetHotelImageByIdAsync(int imageId);
        Task<ApiResponse<object>> DeleteHotelImageAsync(int imageId);
    }
}
