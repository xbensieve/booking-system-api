using AutoMapper;
using Booking.Application.DTOs.Common;
using Booking.Application.DTOs.Hotel;
using Booking.Application.Interfaces;
using Booking.Domain.Entities;
using Booking.Domain.Interfaces;

namespace Booking.Application.Implementations
{
    public class HotelImageService : IHotelImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public HotelImageService(IUnitOfWork unitOfWork, IHotelService hotelService, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponse<object>> AddHotelImagesAsync(int hotelId, List<HotelImageRequest> images)
        {
            var hotel = await _unitOfWork.Hotels.GetByIdAsync(hotelId);
            if (hotel == null)
            {
                return ApiResponse<object>.Fail("Hotel not found.");
            }

            List<string> imageUrls = new List<string>();

            foreach (var item in images)
            {
                var image = new HotelImage
                {
                    HotelId = hotel.Id,
                    ImageUrl = item.ImageUrl,
                    IsMain = false,
                };
                await _unitOfWork.HotelImages.AddAsync(image);
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
                    return ApiResponse<object>.Fail("Failed to addd images. Please try again.");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail("An error occurred while adding images.", ex.Message);
            }
        }

        public async Task<ApiResponse<object>> DeleteHotelImageAsync(int imageId)
        {
            var image = await _unitOfWork.HotelImages.GetByIdAsync(imageId);

            if (image == null)
            {
                return ApiResponse<object>.Fail("Image not found.");
            }

            try
            {
                _unitOfWork.HotelImages.Delete(image);
                int result = await _unitOfWork.SaveChangesAsync();

                if (result > 0)
                {
                    return ApiResponse<object>.Ok(new { imageId }, "Image deleted successfully.");
                }
                else
                {
                    return ApiResponse<object>.Fail("Failed to delete image. Please try again.");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail("An error occurred while deleting the image.", ex.Message);
            }
        }

        public async Task<ApiResponse<HotelImageResponse>> GetHotelImageByIdAsync(int imageId)
        {
            var image = await _unitOfWork.HotelImages.GetByIdAsync(imageId);
            var imageDto = _mapper.Map<HotelImageResponse>(image);

            return imageDto != null
                ? ApiResponse<HotelImageResponse>.Ok(imageDto, "Image retrieved successfully.")
                : ApiResponse<HotelImageResponse>.Fail("Image not found.");
        }

        public async Task<ApiResponse<object>> UpdateHotelImageFileAsync(int imageId, string newImageUrl, bool? isMain = null)
        {
            var image = await _unitOfWork.HotelImages.GetByIdAsync(imageId);
            if (image == null)
                return ApiResponse<object>.Fail("Hotel image not found.");

            image.ImageUrl = newImageUrl;
            if (isMain.HasValue)
                image.IsMain = isMain.Value;

            try
            {
                _unitOfWork.HotelImages.Update(image);
                int result = await _unitOfWork.SaveChangesAsync();
                return result > 0
                    ? ApiResponse<object>.Ok(new { imageId = image.Id, imageUrl = image.ImageUrl }, "Hotel image updated successfully.")
                    : ApiResponse<object>.Fail("Failed to update hotel image.");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail("An error occurred while updating the hotel image.", ex.Message);
            }
        }
    }
}
