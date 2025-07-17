using Booking.Application.DTOs.Review;
using Booking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Api.Controllers
{
    [Route("api/reviews")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ReviewRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _reviewService.CreateReviewAsync(userId, request);
            return (response.Success) ? Ok(response) : BadRequest(response);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ReviewUpdate request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _reviewService.UpdateReviewAsync(userId, id, request);
            return (response.Success) ? Ok(response) : BadRequest(response);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _reviewService.DeleteReviewAsync(userId, id);
            return (response.Success) ? Ok(response) : BadRequest(response);
        }
    }
}
