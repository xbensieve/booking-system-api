using AutoMapper;
using Booking.Repository.Interfaces;
using Booking.Repository.Models;
using Booking.Service.Interfaces;
using Booking.Service.Models;

namespace Booking.Service.Implementations
{
    public class RoomImageService : IRoomImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public RoomImageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ApiResponse<object>> AddRoomImagesAsync(int roomId, List<RoomImageRequest> images)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(roomId);
            if (room == null)
            {
                return ApiResponse<object>.Fail("Room not found.");
            }
            List<string> imageUrls = new List<string>();
            foreach (var item in images)
            {
                var image = new RoomImage
                {
                    RoomId = room.Id,
                    ImageUrl = item.ImageUrl,
                    IsMain = false,
                };
                await _unitOfWork.RoomImages.AddAsync(image);
                imageUrls.Add(image.ImageUrl);
            }
            try
            {
                int result = await _unitOfWork.SaveChangesAsync();
                if (result > 0)
                {
                    return ApiResponse<object>.Ok(new { imageUrls }, "Images added successfully.");
                }
                else
                {
                    return ApiResponse<object>.Fail("Failed to add images. Please try again.");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail("An error occurred while adding images.", ex.Message);
            }
        }

        public async Task<ApiResponse<object>> DeleteRoomImageAsync(int imageId)
        {
            var image = await _unitOfWork.RoomImages.GetByIdAsync(imageId);
            if (image == null)
            {
                return ApiResponse<object>.Fail("Image not found.");
            }

            try
            {
                _unitOfWork.RoomImages.Delete(image);
                int result = await _unitOfWork.SaveChangesAsync();
                return result > 0
                    ? ApiResponse<object>.Ok(new { imageId }, "Image deleted successfully.")
                    : ApiResponse<object>.Fail("Failed to delete image.");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail("An error occurred while deleting the image.", ex.Message);
            }
        }

        public async Task<ApiResponse<RoomImageResponse>> GetRoomImageByIdAsync(int imageId)
        {
            var image = await _unitOfWork.RoomImages.GetByIdAsync(imageId);
            if (image == null)
            {
                return ApiResponse<RoomImageResponse>.Fail("Image not found.");
            }
            var imageResponse = _mapper.Map<RoomImageResponse>(image);
            return ApiResponse<RoomImageResponse>.Ok(imageResponse, "Image retrieved successfully.");
        }

        public async Task<ApiResponse<object>> UpdateRoomImageFileAsync(int imageId, string newImageUrl, bool? isMain = null)
        {
            var image = await _unitOfWork.RoomImages.GetByIdAsync(imageId);
            if (image == null)
            {
                return ApiResponse<object>.Fail("Image not found.");
            }
            image.ImageUrl = newImageUrl;
            if (isMain.HasValue)
            {
                image.IsMain = isMain.Value;
            }
            try
            {
                _unitOfWork.RoomImages.Update(image);
                int result = await _unitOfWork.SaveChangesAsync();
                return result > 0
                    ? ApiResponse<object>.Ok(new { imageId, newImageUrl }, "Image updated successfully.")
                    : ApiResponse<object>.Fail("Failed to update image.");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail("An error occurred while updating the image.", ex.Message);
            }
        }
    }
}
