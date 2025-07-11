using Booking.Service.Models;

namespace Booking.Service.Interfaces
{
    public interface IRoomService
    {
        Task<ApiResponse<object>> AddRoomAsync(int hotelId, RoomRequest request);
        Task<ApiResponse<object>> UpdateRoomAsync(int roomId, RoomRequest request);
        Task<ApiResponse<object>> DeleteRoomAsync(int roomId);
        Task<ApiResponse<List<RoomResponse>>> GetRoomsByHotelIdAsync(int hotelId, int page = 1, int pageSize = 10);
        Task<ApiResponse<RoomResponse>> GetRoomByIdAsync(int roomId);
        Task<ApiResponse<List<RoomResponse>>> SearchRoomsAsync(RoomSearchRequest request);
    }
}
