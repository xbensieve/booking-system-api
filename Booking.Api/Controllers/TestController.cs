using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Api.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        [Authorize]
        [HttpGet("protected")]
        public IActionResult ProtectedGet()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Ok(new
            {
                message = "Protected GET accessed successfully",
                userId
            });
        }

        [Authorize]
        [HttpPost("protected")]
        public IActionResult ProtectedPost([FromBody] object data)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Ok(new
            {
                message = "Protected POST accessed successfully",
                dataReceived = data,
                userId
            });
        }

        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok(new
            {
                message = "This is a public endpoint"
            });
        }
    }
}
