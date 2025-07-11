using Booking.Service.Models;

namespace Booking.Service.Interfaces
{
    public interface IRoomImageService
    {
        Task<ApiResponse<object>> AddRoomImagesAsync(int roomId, List<RoomImageRequest> images);
        Task<ApiResponse<RoomImageResponse>> GetRoomImageByIdAsync(int imageId);
        Task<ApiResponse<object>> DeleteRoomImageAsync(int imageId);
        Task<ApiResponse<object>> UpdateRoomImageFileAsync(int imageId, string newImageUrl, bool? isMain = null);
    }
}
