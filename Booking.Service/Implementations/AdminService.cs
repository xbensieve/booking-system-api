using AutoMapper;
using Booking.Repository.Interfaces;
using Booking.Service.Interfaces;
using Booking.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Booking.Service.Implementations
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

    }
}
