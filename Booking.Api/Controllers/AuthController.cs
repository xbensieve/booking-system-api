using Microsoft.AspNetCore.Mvc;

namespace Booking.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
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
    }
}
