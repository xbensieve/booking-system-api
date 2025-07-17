using Booking.Application.DTOs.ML;
using Booking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Api.Controllers
{
    [Route("api/ml")]
    [ApiController]
    public class MLRecommendationController : ControllerBase
    {
        private readonly IMLService _mlService;
        public MLRecommendationController(IMLService mlService)
        {
            _mlService = mlService;
        }

        [HttpPost("recommend")]
        public IActionResult Recommend([FromBody] SearchData data)
        {
            var result = _mlService.Predict(data);
            return Ok(result);
        }
    }
}
