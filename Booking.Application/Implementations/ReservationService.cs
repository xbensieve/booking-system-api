using AutoMapper;
using Booking.Application.DTOs.Common;
using Booking.Application.DTOs.Price;
using Booking.Application.DTOs.Reservation;
using Booking.Application.Interfaces;
using Booking.Domain.Entities;
using Booking.Domain.Enums;
using Booking.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Implementations
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
        public async Task<ApiResponse<object>> CancelReservationAsync(int reservationId)
        {
            var reservation = await _unitOfWork.Reservations.Query()
                .Include(r => r.Room)
                .Where(r => r.Id == reservationId && !r.IsDeleted)
                .FirstOrDefaultAsync();

            if (reservation == null)
            {
                return ApiResponse<object>.Fail("Reservation not found.");
            }
            if (reservation.Status == BookingStatus.Cancelled)
            {
                return ApiResponse<object>.Fail("Reservation is already cancelled.");
            }
            reservation.Status = BookingStatus.Cancelled;
            reservation.Room.IsAvailable = true;
            reservation.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                int result = await _unitOfWork.SaveChangesAsync();
                if (result > 0)
                {
                    await _unitOfWork.CommitTransactionAsync();
                    return ApiResponse<object>.Ok(new { reservationId }, "Reservation cancelled successfully.");
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<object>.Fail("Failed to cancel reservation.");
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<object>.Fail($"An error occurred while cancelling the reservation: {ex.Message}");
            }
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

        public async Task<ApiResponse<object>> CheckInReservationAsync(int reservationId, DateTime checkInTime)
        {
            var reservation = await _unitOfWork.Reservations.GetByIdAsync(reservationId);
            if (reservation == null)
            {
                return ApiResponse<object>.Fail("Reservation not found.");
            }
            if (reservation.Status != BookingStatus.Confirmed)
            {
                return ApiResponse<object>.Fail("Reservation is not in a valid state for check-in.");
            }

            var currentTime = DateTime.UtcNow;
            var maxCheckInTime = reservation.CheckInDate.AddHours(24);
            // Check if the check-in time is within the allowed range
            if (currentTime > maxCheckInTime)
            {
                return ApiResponse<object>.Fail("Check-in time has expired. Please contact support.");
            }
            // Check if the check-in time is before the reservation's check-in date
            double earlyCheckInHours = (reservation.CheckInDate - checkInTime).TotalHours;
            string message = $"{(earlyCheckInHours > 0 ? $"Early check-in by {earlyCheckInHours:F2} hours" : "On-time check-in")}";

            reservation.Status = BookingStatus.CheckedIn;
            reservation.CheckInTime = checkInTime;
            reservation.Note = message;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                int result = await _unitOfWork.SaveChangesAsync();
                if (result > 0)
                {
                    await _unitOfWork.CommitTransactionAsync();
                    return ApiResponse<object>.Ok(new { reservationId }, "Check-in successful.");
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<object>.Fail("Failed to check-in reservation.");
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<object>.Fail($"An error occurred while checking in: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> CheckOutReservationAsync(int reservationId, DateTime checkOutTime)
        {
            var reservation = await _unitOfWork.Reservations.Query()
                .Include(r => r.Room)
                .Where(r => r.Id == reservationId && r.Status == BookingStatus.CheckedIn)
                .FirstOrDefaultAsync();

            if (reservation == null)
            {
                return ApiResponse<object>.Fail("Reservation not found.");
            }

            if (reservation.Status != BookingStatus.CheckedIn)
            {
                return ApiResponse<object>.Fail("Reservation is not in a valid state for check-out.");
            }
            if (!reservation.CheckInTime.HasValue)
            {
                return ApiResponse<object>.Fail("Check-in/check-out time is missing and required for surcharge calculation.");
            }

            double lateCheckOutHours = (checkOutTime - reservation.CheckOutDate).TotalHours;
            string message = $"{(lateCheckOutHours > 0 ? $"Late check-out by {lateCheckOutHours:F2} hours" : "On-time check-out")}";
            var pricingCalculator = new PricingCalculator();
            var surcharge = pricingCalculator.CalculateDetailedRoomPrice(
                reservation.CheckInTime.Value,
                reservation.CheckInDate,
                checkOutTime,
                reservation.CheckOutDate,
                reservation.Room.PricePerNight);

            if (surcharge == null)
            {
                return ApiResponse<object>.Fail("Failed to calculate surcharges for check-out.");
            }

            reservation.Status = BookingStatus.CheckedOut;
            reservation.CheckOutTime = checkOutTime;
            reservation.Note += "\n" + message;
            reservation.UpdatedAt = DateTime.UtcNow;
            reservation.EarlyCheckInSurcharge = surcharge.EarlyCheckInFee;
            reservation.LateCheckOutSurcharge = surcharge.LateCheckOutFee;
            reservation.ActualPrice = surcharge.Total;
            reservation.Room.IsAvailable = true;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                int result = await _unitOfWork.SaveChangesAsync();
                if (result > 0)
                {
                    await _unitOfWork.CommitTransactionAsync();
                    return ApiResponse<object>.Ok(new { reservationId }, "Check-out successful.");
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<object>.Fail("Failed to check-out reservation.");
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<object>.Fail($"An error occurred while checking out: {ex.Message}");
            }
        }
    }
}
