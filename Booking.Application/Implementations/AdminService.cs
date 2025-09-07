using AutoMapper;
using Booking.Application.DTOs.Common;
using Booking.Application.DTOs.Reservation;
using Booking.Application.Interfaces;
using Booking.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public AdminService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<ReservationResponse>>> GetReservationsByDateAsync(DateTime today, int page, int pageSize)
        {
            var query = _unitOfWork.Reservations.Query()
                .Include(r => r.User)
                .Include(r => r.Room)
                .Where(r => r.CheckInDate.Date == today.Date)
                .OrderByDescending(r => r.CheckInDate)
                .AsQueryable();

            int total = await query.CountAsync();

            var reservations = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapped = _mapper.Map<List<ReservationResponse>>(reservations);

            return ApiResponse<List<ReservationResponse>>.Ok(mapped, $"Found {total} reservations on {today:yyyy-MM-dd}.");
        }

        public async Task<ApiResponse<object>> GetReservationStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            var reservationCount = await _unitOfWork.Reservations.Query()
                .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
                .CountAsync();

            return new ApiResponse<object>
            {
                Success = true,
                Data = new { ReservationCount = reservationCount }
            };
        }

        public async Task<ApiResponse<object>> GetRevenueStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            var revenue = await _unitOfWork.Payments.Query()
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate && p.Status == Domain.Enums.PaymentStatus.Completed)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            return new ApiResponse<object>
            {
                Success = true,
                Data = new { Revenue = revenue }
            };
        }

        public async Task<ApiResponse<object>> GetReviewStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            var reviewCount = await _unitOfWork.Reviews.Query()
               .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
               .CountAsync();

            return new ApiResponse<object>
            {
                Success = true,
                Data = new { ReviewCount = reviewCount }
            };
        }
    }
}
