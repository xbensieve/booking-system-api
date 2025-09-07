using AutoMapper;
using Booking.Application.DTOs.Common;
using Booking.Application.DTOs.Review;
using Booking.Application.Interfaces;
using Booking.Domain.Entities;
using Booking.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHotelService _hotelService;
        private readonly IMapper _mapper;
        public ReviewService(IUnitOfWork unitOfWork, IHotelService hotelService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _hotelService = hotelService;
            _mapper = mapper;
        }
        public async Task<ApiResponse<ReviewResponse>> CreateReviewAsync(Guid userId, ReviewRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return ApiResponse<ReviewResponse>.Fail("User not found");

            var hotel = await _unitOfWork.Hotels.GetByIdAsync(request.HotelId);
            if (hotel == null) return ApiResponse<ReviewResponse>.Fail("Hotel not found");

            var review = new Review
            {
                HotelId = hotel.Id,
                UserId = user.UserId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.Now,
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _unitOfWork.Reviews.AddAsync(review);

                await _hotelService.UpdateHotelRatingAsync(hotel.Id, review.Rating);

                int result = await _unitOfWork.SaveChangesAsync();
                var reviewDto = _mapper.Map<ReviewResponse>(review);
                if (result > 0)
                {
                    await _unitOfWork.CommitTransactionAsync();
                    return ApiResponse<ReviewResponse>.Ok(reviewDto, "Review created successfully.");
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<ReviewResponse>.Fail("Failed to create review. Please try again.");
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<ReviewResponse>.Fail(ex.Message);
            }
        }

        public async Task<ApiResponse<object>> DeleteReviewAsync(Guid userId, int reviewId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return ApiResponse<object>.Fail("User not found");


            var review = await _unitOfWork.Reviews.Query()
                .Include(r => r.User)
                .Where(r => r.UserId == userId && r.Id == reviewId)
                .FirstOrDefaultAsync();

            if (review == null) return ApiResponse<object>.Fail("Review not found");

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                _unitOfWork.Reviews.Delete(review);
                await _hotelService.UpdateHotelRatingOnReviewDeletedAsync(review.HotelId, review.Rating);

                int result = await _unitOfWork.SaveChangesAsync();
                if (result > 0)
                {
                    await _unitOfWork.CommitTransactionAsync();
                    return ApiResponse<object>.Ok("Review deleted successfully.");
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<object>.Fail("Failed to delete review. Please try again.");
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<object>.Fail(ex.Message);
            }
        }

        public async Task<ApiResponse<List<ReviewResponse>>> GetReviewsByHotelIdAsync(int hotelId, int page, int pageSize)
        {
            var hotel = await _unitOfWork.Hotels.GetByIdAsync(hotelId);
            if (hotel == null) return ApiResponse<List<ReviewResponse>>.Fail("Hotel not found");

            var reviewQuery = _unitOfWork.Reviews.Query()
                .Include(r => r.User)
                .Include(r => r.Hotel);
            var totalReviews = reviewQuery.Count();
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            var totalPages = (int)Math.Ceiling((double)totalReviews / pageSize);
            var reviews = await reviewQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var reviewsDtos = _mapper.Map<List<ReviewResponse>>(reviews);

            return ApiResponse<List<ReviewResponse>>.Ok(reviewsDtos, "Reviews retrieved successfully.");
        }

        public async Task<ApiResponse<ReviewResponse>> UpdateReviewAsync(Guid userId, int reviewId, ReviewUpdate request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return ApiResponse<ReviewResponse>.Fail("User not found");


            var review = await _unitOfWork.Reviews.Query()
                .Include(r => r.User)
                .Where(r => r.UserId == userId && r.Id == reviewId)
                .FirstOrDefaultAsync();
            if (review == null) return ApiResponse<ReviewResponse>.Fail("Review not found");

            review.Rating = request.Rating;
            review.Comment = request.Comment;


            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _hotelService.UpdateHotelRatingOnReviewEditAsync(review.HotelId, review.Rating, request.Rating);

                int result = await _unitOfWork.SaveChangesAsync();
                var reviewDto = _mapper.Map<ReviewResponse>(review);
                if (result > 0)
                {
                    await _unitOfWork.CommitTransactionAsync();
                    return ApiResponse<ReviewResponse>.Ok(reviewDto, "Review updated successfully.");
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<ReviewResponse>.Fail("Failed to update review. Please try again.");
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<ReviewResponse>.Fail(ex.Message);
            }
        }
    }
}
