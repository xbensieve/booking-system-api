using Booking.Repository.Interfaces;
using Booking.Service.Interfaces;
using Booking.Service.Models;
using Microsoft.Extensions.Configuration;
using VNPAY;

namespace Booking.Service.Implementations
{
    public class VnPayService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public VnPayService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }
        public async Task<ApiResponse<object>> CreatePaymentUrlAsync(OrderInfo order)
        {
            var reservation = await _unitOfWork.Reservations.GetByIdAsync(order.ReservationId);
            if (reservation == null) return ApiResponse<object>.Fail("Reservation not found");
            var ipAddress = "127.0.0.1";
            var tmnCode = _configuration["VNP_TMN_CODE"];
            var hashSecret = _configuration["VNP_HASH_SECRET"];
            var command = _configuration["VNP_COMMAND"];
            var currCode = _configuration["VNP_CURRCODE"];
            var version = _configuration["VNP_VERSION"];
            var locale = _configuration["VNP_LOCALE"];
            var baseUrl = _configuration["VNP_BASE_URL"];
            var returnUrl = _configuration["VNP_RETURN_URL"];
            var orderInfo = $"Payment";

            VnPayLibrary vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", tmnCode);
            vnpay.AddRequestData("vnp_Amount", (Decimal.ToInt64(reservation.TotalPrice) * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + reservation.Id.ToString());
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", reservation.Id.ToString());
            string paymentUrl = vnpay.CreateRequestUrl(baseUrl, hashSecret);
            return ApiResponse<object>.Ok(new { Url = paymentUrl, ReservationId = reservation.Id });
        }

    }
}
