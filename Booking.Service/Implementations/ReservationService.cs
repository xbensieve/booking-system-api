using AutoMapper;
using Booking.Repository.Enums;
using Booking.Repository.Interfaces;
using Booking.Repository.Models;
using Booking.Service.Interfaces;
using Booking.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Booking.Service.Implementations
{
    public class ReservationService : IReservationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ReservationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponse<object>> CreateReservationAsync(string UserId, ReservationRequest request)
        {
            if (request == null) return ApiResponse<object>.Fail("Reservation request cannot be null.");

            var user = await _unitOfWork.Users.GetByIdAsync(UserId);
            if (user == null)
            {
                return ApiResponse<object>.Fail("User not found.");
            }

            var room = await _unitOfWork.Rooms.GetByIdAsync(request.RoomId);
            if (room == null)
            {
                return ApiResponse<object>.Fail("Room not found.");
            }

            if (room.IsAvailable == false)
            {
                return ApiResponse<object>.Fail("Room is not available for reservation.");
            }

            if (request.CheckInDate >= request.CheckOutDate)
            {
                return ApiResponse<object>.Fail("Check-in date must be before check-out date.");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                bool isConflict = await _unitOfWork.Reservations.ExistsAsync(b =>
                b.RoomId == request.RoomId &&
                b.Status != BookingStatus.Cancelled &&
                b.CheckInDate < request.CheckOutDate &&
                b.CheckOutDate > request.CheckInDate);

                if (isConflict)
                {
                    return ApiResponse<object>.Fail("Room is already reserved for the selected dates.");
                }


                var reservation = new Reservation
                {
                    UserUid = UserId,
                    RoomId = request.RoomId,
                    CheckInDate = request.CheckInDate,
                    CheckOutDate = request.CheckOutDate,
                    TotalPrice = CalculateTotalPrice(request.CheckInDate, request.CheckOutDate, room.PricePerNight),
                    NumberOfGuests = request.NumberOfGuests,
                    Status = BookingStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                };

                await _unitOfWork.Reservations.AddAsync(reservation);
                int result = await _unitOfWork.SaveChangesAsync();
                if (result > 0)
                {
                    await _unitOfWork.CommitTransactionAsync();
                    return ApiResponse<object>.Ok(new { reservationId = reservation.Id }, "Reservation created successfully.");
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<object>.Fail("Failed to create reservation.");
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<object>.Fail($"An error occurred while creating the reservation: {ex.Message}");
            }
        }
        public Task CancelReservationAsync(int reservationId)
        {
            throw new NotImplementedException();
        }
        private decimal CalculateTotalPrice(DateTime checkIn, DateTime checkOut, decimal pricePerNight)
        {
            int numberOfNights = (checkOut.Date - checkIn.Date).Days;
            return pricePerNight * numberOfNights;
        }

        public async Task<ApiResponse<ReservationResponse>> GetReservationByIdAsync(int reservationId)
        {
            var reservation = await _unitOfWork.Reservations.Query().Include(r => r.User).Include(r => r.Room).Where(r => r.Id == reservationId).FirstOrDefaultAsync();
            if (reservation == null)
            {
                return ApiResponse<ReservationResponse>.Fail("Reservation not found.");
            }
            var room = await _unitOfWork.Rooms.GetByIdAsync(reservation.RoomId);
            if (room == null)
            {
                return ApiResponse<ReservationResponse>.Fail("Room not found.");
            }
            var user = await _unitOfWork.Users.GetByIdAsync(reservation.UserUid);
            if (user == null)
            {
                return ApiResponse<ReservationResponse>.Fail("User not found.");
            }
            var response = _mapper.Map<ReservationResponse>(reservation);
            return ApiResponse<ReservationResponse>.Ok(response, "Reservation retrieved successfully.");
        }

        public async Task<ApiResponse<List<ReservationResponse>>> GetReservationsByUserIdAsync(string userId, int page, int pageSize)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return ApiResponse<List<ReservationResponse>>.Fail("User ID cannot be null or empty.");
            }
            var reservations = await _unitOfWork.Reservations.Query()
                .Where(r => r.UserUid == userId && !r.IsDeleted)
                .Include(r => r.User)
                .Include(r => r.Room)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            if (reservations == null || !reservations.Any())
            {
                return ApiResponse<List<ReservationResponse>>.Fail("No reservations found for the user.");
            }
            var response = _mapper.Map<List<ReservationResponse>>(reservations);
            return ApiResponse<List<ReservationResponse>>.Ok(response, "Reservations retrieved successfully.");
        }
    }
}
