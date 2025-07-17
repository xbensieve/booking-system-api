using Booking.Application.DTOs.Common;
using Booking.Application.DTOs.Review;

namespace Booking.Application.Interfaces
{
    public interface IReviewService
    {
        Task<ApiResponse<ReviewResponse>> CreateReviewAsync(string userId, ReviewRequest request);
        Task<ApiResponse<ReviewResponse>> UpdateReviewAsync(string userId, int reviewId, ReviewUpdate request);
        Task<ApiResponse<object>> DeleteReviewAsync(string userId, int reviewId);
        Task<ApiResponse<List<ReviewResponse>>> GetReviewsByHotelIdAsync(int hotelId, int page, int pageSize);
    }
}
