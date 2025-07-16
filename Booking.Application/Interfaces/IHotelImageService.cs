using Booking.Application.DTOs.Common;
using Booking.Application.DTOs.Hotel;

namespace Booking.Application.Interfaces
{
    public interface IHotelImageService
    {
        Task<ApiResponse<object>> AddHotelImagesAsync(int hotelId, List<HotelImageRequest> images);
        Task<ApiResponse<HotelImageResponse>> GetHotelImageByIdAsync(int imageId);
        Task<ApiResponse<object>> DeleteHotelImageAsync(int imageId);
        Task<ApiResponse<object>> UpdateHotelImageFileAsync(int imageId, string newImageUrl, bool? isMain = null);
    }
}
