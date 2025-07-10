using Booking.Service.Interfaces;
using Booking.Service.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Booking.Api.Controllers
{
    [Route("api/hotels")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        public HotelController(IHotelService hotelService)
        {
            _hotelService = hotelService ?? throw new ArgumentNullException(nameof(hotelService));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var response = await _hotelService.GetHotelByIdAsync(id);
            return response.Success
                ? Ok(response.Data)
                : NotFound(response.Message);
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] HotelRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _hotelService.CreateHotelAsync(request);

            return response.Success
                ? Ok(response.Data)
                : BadRequest(response.Message);
        }

    }
}
