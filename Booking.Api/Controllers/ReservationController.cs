using Booking.Application.DTOs.Reservation;
using Booking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Booking.Api.Controllers
{
    [Route("api/reservations")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
        }
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> CreateReservationAsync([FromBody] ReservationRequest request)
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var userId))
                return Unauthorized("Invalid user ID in token.");

            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var response = await _reservationService.CreateReservationAsync(userId, request);
            return response.Success
                 ? Ok(response)
                 : BadRequest(response.Message);
        }
        [HttpGet("{reservationId}")]
        public async Task<IActionResult> GetReservationByIdAsync(int reservationId)
        {
            var response = await _reservationService.GetReservationByIdAsync(reservationId);
            return response.Success
                 ? Ok(response)
                 : NotFound(response.Message);
        }
        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetReservationsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("User ID is required.");
            }
            var response = await _reservationService.GetReservationsByUserIdAsync(userId, page, pageSize);
            return response.Success
                 ? Ok(response)
                 : NotFound(response.Message);
        }
        [Authorize]
        [HttpDelete("{reservationId}")]
        public async Task<IActionResult> CancelReservationAsync(int reservationId)
        {
            var response = await _reservationService.CancelReservationAsync(reservationId);
            return response.Success
                 ? Ok(response)
                 : NotFound(response.Message);
        }
        [Authorize]
        [HttpPost("{reservationId}/check-in")]
        public async Task<IActionResult> CheckInReservationAsync(int reservationId, [FromBody] DateTime checkInTime)
        {
            if (checkInTime == default)
            {
                return BadRequest("Invalid check-in time.");
            }
            var response = await _reservationService.CheckInReservationAsync(reservationId, checkInTime);
            return response.Success
                 ? Ok(response)
                 : NotFound(response.Message);
        }
        [Authorize]
        [HttpPost("{reservationId}/check-out")]
        public async Task<IActionResult> CheckOutReservationAsync(int reservationId, [FromBody] DateTime checkOutTime)
        {
            if (checkOutTime == default)
            {
                return BadRequest("Invalid check-out time.");
            }
            var response = await _reservationService.CheckOutReservationAsync(reservationId, checkOutTime);
            return response.Success
                 ? Ok(response)
                 : NotFound(response.Message);
        }
    }
}
