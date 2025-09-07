using AutoMapper;
using Booking.Application.DTOs.Common;
using Booking.Application.DTOs.Room;
using Booking.Application.Interfaces;
using Booking.Domain.Entities;
using Booking.Domain.Enums;
using Booking.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Implementations
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public RoomService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponse<object>> AddRoomAsync(int hotelId, RoomRequest request)
        {
            var hotel = await _unitOfWork.Hotels.GetByIdAsync(hotelId);
            if (hotel == null) return ApiResponse<object>.Fail("Hotel not found");

            var room = new Room
            {
                HotelId = hotelId,
                RoomNumber = request.RoomNumber,
                Description = request.Description,
                PricePerNight = request.PricePerNight,
                Amenities = request.Amenities ?? string.Empty,
                Capacity = request.Capacity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            try
            {
                await _unitOfWork.Rooms.AddAsync(room);
                int result = await _unitOfWork.SaveChangesAsync();
                return result > 0
                    ? ApiResponse<object>.Ok(new { room.Id }, "Room created successfully")
                    : ApiResponse<object>.Fail("Failed to create room");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail($"Error creating room: {ex.Message}");
            }
        }
        public async Task<ApiResponse<object>> DeleteRoomAsync(int roomId)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(roomId);
            if (room == null) return ApiResponse<object>.Fail("Room not found");

            room.IsDeleted = true;
            room.UpdatedAt = DateTime.UtcNow;
            try
            {
                int result = await _unitOfWork.SaveChangesAsync();
                return (result > 0)
                    ? ApiResponse<object>.Ok(new { room.Id }, "Room deleted successfully")
                    : ApiResponse<object>.Fail("Failed to delete room");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail($"Error deleting room: {ex.Message}");
            }

        }
        public async Task<ApiResponse<RoomResponse>> GetRoomByIdAsync(int roomId)
        {
            var room = await _unitOfWork.Rooms.Query().Where(r => r.Id == roomId && !r.IsDeleted)
                .Include(r => r.Images)
                .FirstOrDefaultAsync();
            if (room == null) return ApiResponse<RoomResponse>.Fail("Room not found");

            var roomResponse = _mapper.Map<RoomResponse>(room);

            return ApiResponse<RoomResponse>.Ok(roomResponse, "Room retrieved successfully");
        }
        public async Task<ApiResponse<List<RoomResponse>>> GetRoomsByHotelIdAsync(int hotelId, int page = 1, int pageSize = 10)
        {
            var hotel = await _unitOfWork.Hotels.GetByIdAsync(hotelId);
            if (hotel == null) return ApiResponse<List<RoomResponse>>.Fail("Hotel not found");

            var roomsQuery = _unitOfWork.Rooms.Query()
                .Where(r => r.HotelId == hotelId && !r.IsDeleted)
                .Include(r => r.Images);
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            var totalRooms = roomsQuery.Count();
            var totalPages = (int)Math.Ceiling((double)totalRooms / pageSize);
            var rooms = await roomsQuery
                .OrderBy(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var roomResponses = _mapper.Map<List<RoomResponse>>(rooms);

            return ApiResponse<List<RoomResponse>>.Ok(roomResponses, "Rooms retrieved successfully");
        }
        public async Task<ApiResponse<List<RoomResponse>>> SearchRoomsAsync(RoomSearchRequest request)
        {
            var query = _unitOfWork.Rooms.Query()
                .Include(r => r.Hotel)
                .Include(r => r.Images)
                .Where(r =>
                    r.Capacity >= request.NumberOfPeople
                );
            var rooms = await query.ToListAsync();
            if (rooms == null || !rooms.Any()) return ApiResponse<List<RoomResponse>>.Fail("No rooms found matching the criteria");

            var result = new List<Room>();

            foreach (var room in rooms)
            {
                bool isBooked = await _unitOfWork.Reservations.Query()
                    .AnyAsync(res =>
                        res.RoomId == room.Id &&
                        !res.IsDeleted &&
                        res.Status != BookingStatus.Cancelled &&
                        (
                            (request.CheckInDate >= res.CheckInDate && request.CheckInDate < res.CheckOutDate) ||
                            (request.CheckOutDate > res.CheckInDate && request.CheckOutDate <= res.CheckOutDate) ||
                            (request.CheckInDate <= res.CheckInDate && request.CheckOutDate >= res.CheckOutDate)
                        )
                    );

                if (!isBooked)
                {
                    result.Add(room);
                }
            }
            var roomDtos = result
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var roomResponses = _mapper.Map<List<RoomResponse>>(roomDtos);
            return ApiResponse<List<RoomResponse>>.Ok(roomResponses, "Rooms retrieved successfully");
        }
        public async Task<ApiResponse<object>> UpdateRoomAsync(int roomId, RoomRequest request)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(roomId);
            if (room == null) return ApiResponse<object>.Fail("Room not found");

            room.RoomNumber = request.RoomNumber;
            room.Description = request.Description;
            room.PricePerNight = request.PricePerNight;
            room.Amenities = request.Amenities ?? string.Empty;
            room.Capacity = request.Capacity;
            room.UpdatedAt = DateTime.UtcNow;
            try
            {
                int result = await _unitOfWork.SaveChangesAsync();
                return (result > 0)
                    ? ApiResponse<object>.Ok(new { room.Id }, "Room updated successfully")
                    : ApiResponse<object>.Fail("Failed to update room");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail($"Error updating room: {ex.Message}");
            }
        }
    }
}
