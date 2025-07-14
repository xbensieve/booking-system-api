using Booking.Service.Interfaces;
using Booking.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Booking.Api.Controllers
{
    [Route("api/rooms")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        public RoomController(IRoomService roomService)
        {
            _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("{hotelId}")]
        public async Task<IActionResult> Post(int hotelId, [FromBody] RoomRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _roomService.AddRoomAsync(hotelId, request);
            return response.Success
                ? Ok(response)
                : BadRequest(response.Message);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{roomId}")]
        public async Task<IActionResult> Put(int roomId, [FromBody] RoomRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _roomService.UpdateRoomAsync(roomId, request);
            return response.Success
                ? Ok(response)
                : BadRequest(response.Message);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{roomId}")]
        public async Task<IActionResult> Delete(int roomId)
        {
            var response = await _roomService.DeleteRoomAsync(roomId);
            return response.Success
                ? Ok(response)
                : BadRequest(response.Message);
        }
        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetByHotelId(int hotelId, int page = 1, int pageSize = 10)
        {
            var response = await _roomService.GetRoomsByHotelIdAsync(hotelId, page, pageSize);
            return response.Success
                ? Ok(response)
                : NotFound(response.Message);
        }
        [HttpGet("{roomId}")]
        public async Task<IActionResult> GetById(int roomId)
        {
            var response = await _roomService.GetRoomByIdAsync(roomId);
            return response.Success
                ? Ok(response)
                : NotFound(response.Message);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] RoomSearchRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _roomService.SearchRoomsAsync(request);
            return response.Success
                ? Ok(response)
                : BadRequest(response.Message);
        }
    }
}
