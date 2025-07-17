using Booking.Application.DTOs.Hotel;
using Booking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Booking.Api.Controllers
{
    [Route("api/hotels")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly IReviewService _reviewService;
        public HotelController(IHotelService hotelService, IReviewService reviewService)
        {
            _hotelService = hotelService ?? throw new ArgumentNullException(nameof(hotelService));
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var response = await _hotelService.GetAllHotelsAsync(page, pageSize);
            return response.Success
                ? Ok(response)
                : BadRequest(response.Message);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] HotelRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _hotelService.UpdateHotelAsync(id, request);
            return response.Success
                ? Ok(response)
                : BadRequest(response.Message);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var response = await _hotelService.GetHotelByIdAsync(id);
            return response.Success
                ? Ok(response)
                : NotFound(response.Message);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] HotelRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _hotelService.CreateHotelAsync(request);

            return response.Success
                ? Ok(response)
                : BadRequest(response.Message);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _hotelService.DeleteHotelAsync(id);
            return response.Success
                ? Ok(response)
                : NotFound(response.Message);
        }
        [HttpGet("{id}/reviews")]
        public async Task<IActionResult> Get(int id, int page = 1, int pageSize = 10)
        {
            var response = await _reviewService.GetReviewsByHotelIdAsync(id, page, pageSize);
            return (response.Success) ? Ok(response) : BadRequest(response.Message);
        }
    }
}
