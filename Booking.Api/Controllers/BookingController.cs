using Microsoft.AspNetCore.Mvc;

namespace Booking.Api.Controllers
{
    [Route("api/bookings")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok("healthy");
        }
        // Additional booking-related endpoints can be added here
    }
}
