using Booking.Application.DTOs.Review;
using Booking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var userId))
                return Unauthorized("Invalid user ID in token.");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _reviewService.CreateReviewAsync(userId, request);
            return response.Success
                ? Ok(response)
                : BadRequest(response.Message);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ReviewUpdate request)
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var userId))
                return Unauthorized("Invalid user ID in token.");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _reviewService.UpdateReviewAsync(userId, id, request);
            return response.Success
                ? Ok(response)
                : BadRequest(response.Message);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var userId))
                return Unauthorized("Invalid user ID in token.");
            var response = await _reviewService.DeleteReviewAsync(userId, id);
            return response.Success
                ? Ok(response)
                : BadRequest(response.Message);
        }
    }
}
