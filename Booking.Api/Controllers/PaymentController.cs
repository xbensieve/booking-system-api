using Booking.Application.DTOs.Order;
using Booking.Application.DTOs.Payment;
using Booking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Booking.Api.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] OrderInfo request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _paymentService.CreatePaymentUrlAsync(request);
            return (response.Success)
                ? Ok(response)
                : BadRequest(response);
        }
        [HttpGet("handle-payment-response")]
        public async Task<IActionResult> HandlePaymentResponse([FromQuery] PaymentResponse response)
        {
            if (response == null)
            {
                _logger.LogWarning("VNPay callback missing query parameters.");
                return BadRequest("Invalid payment response");
            }

            var result = await _paymentService.HandlePaymentResponseAsync(response);
            return Ok(result);
        }
    }
}
