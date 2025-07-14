using Booking.Service.Interfaces;
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
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }
        [HttpGet("dashboard")]
        public IActionResult Get()
        {
            return Ok("Welcome Admin");
        }
        [HttpGet("reservations/today")]
        public async Task<IActionResult> GetTodayBookings(int page = 1, int pageSize = 10)
        {
            var today = DateTime.Today;
            var bookings = await _adminService.GetReservationsByDateAsync(today, page, pageSize);
            return Ok(bookings);
        }
    }
}
