using Booking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;
        public AdminController(IAdminService adminService, IUserService userService)
        {
            _adminService = adminService;
            _userService = userService;
        }
        [HttpGet("reservations/today")]
        public async Task<IActionResult> GetTodayBookings(int page = 1, int pageSize = 10)
        {
            var today = DateTime.Today;
            var bookings = await _adminService.GetReservationsByDateAsync(today, page, pageSize);
            return Ok(bookings);
        }
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers(int page = 1, int pageSize = 10)
        {
            var users = await _userService.GetAllUsersAsync(page, pageSize);
            return Ok(users);
        }
        [HttpDelete("users/{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var result = await _userService.DeleteUserAsync(userId);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
        [HttpPut("users/{userId}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] Application.DTOs.User.UpdateUserRequest request)
        {
            var result = await _userService.UpdateUserAsync(userId, request);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
        [HttpGet("statistics/reservations")]
        public async Task<IActionResult> GetReservationStatistics(DateTime startDate, DateTime endDate)
        {
            var stats = await _adminService.GetReservationStatisticsAsync(startDate, endDate);
            return Ok(stats);
        }
        [HttpGet("statistics/revenue")]
        public async Task<IActionResult> GetRevenueStatistics(DateTime startDate, DateTime endDate)
        {
            var stats = await _adminService.GetRevenueStatisticsAsync(startDate, endDate);
            return Ok(stats);
        }
        [HttpGet("statistics/reviews")]
        public async Task<IActionResult> GetReviewStatistics(DateTime startDate, DateTime endDate)
        {
            var stats = await _adminService.GetReviewStatisticsAsync(startDate, endDate);
            return Ok(stats);
        }
    }
}
