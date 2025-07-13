using Booking.Repository.Enums;
using Booking.Repository.Interfaces;
using Booking.Repository.Models;
using Booking.Service.Interfaces;
using Booking.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VNPAY;

namespace Booking.Service.Implementations
{
    public class VnPayService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<VnPayService> _logger;
        public VnPayService(IUnitOfWork unitOfWork, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<VnPayService> logger, IBackgroundTaskQueue taskQueue)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _taskQueue = taskQueue;
        }
        public async Task<ApiResponse<object>> CreatePaymentUrlAsync(OrderInfo order)
        {
            var reservation = await _unitOfWork.Reservations.GetByIdAsync(order.ReservationId);
            if (reservation == null)
                return ApiResponse<object>.Fail("Reservation not found");

            var vnPayConfig = new
            {
                TmnCode = _configuration["VNP_TMN_CODE"],
                HashSecret = _configuration["VNP_HASH_SECRET"],
                Command = _configuration["VNP_COMMAND"] ?? "pay",
                CurrencyCode = _configuration["VNP_CURRCODE"] ?? "VND",
                Version = _configuration["VNP_VERSION"] ?? "2.1.0",
                Locale = _configuration["VNP_LOCALE"] ?? "vn",
                BaseUrl = _configuration["VNP_BASE_URL"],
                ReturnUrl = _configuration["VNP_RETURN_URL"]
            };
            if (string.IsNullOrEmpty(vnPayConfig.TmnCode) || string.IsNullOrEmpty(vnPayConfig.HashSecret) ||
                string.IsNullOrEmpty(vnPayConfig.BaseUrl) || string.IsNullOrEmpty(vnPayConfig.ReturnUrl))
            {
                return ApiResponse<object>.Fail("Missing VNPay configuration.");
            }

            var vnpay = new VnPayLibrary();
            var currentTime = DateTime.Now;

            vnpay.AddRequestData("vnp_Version", vnPayConfig.Version);
            vnpay.AddRequestData("vnp_Command", vnPayConfig.Command);
            vnpay.AddRequestData("vnp_TmnCode", vnPayConfig.TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(reservation.TotalPrice * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", currentTime.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", vnPayConfig.CurrencyCode);
            vnpay.AddRequestData("vnp_IpAddr", GetClientIpAddress());
            vnpay.AddRequestData("vnp_Locale", vnPayConfig.Locale);
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan đon hang: {reservation.Id}");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnPayConfig.ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", reservation.Id.ToString());

            var paymentUrl = vnpay.CreateRequestUrl(vnPayConfig.BaseUrl, vnPayConfig.HashSecret);

            if (string.IsNullOrWhiteSpace(paymentUrl))
                return ApiResponse<object>.Fail("Failed to generate VNPay payment URL.");

            return ApiResponse<object>.Ok(new
            {
                Url = paymentUrl,
                ReservationId = reservation.Id
            });
        }

        public async Task<ApiResponse<object>> HandlePaymentResponseAsync(PaymentResponse response)
        {
            try
            {
                if (response.vnp_ResponseCode == "00")
                {
                    var reservation = await _unitOfWork.Reservations.Query()
                        .Include(r => r.Room)
                        .Where(r => r.Id == int.Parse(response.vnp_TxnRef) && r.Status == BookingStatus.Pending)
                        .FirstOrDefaultAsync();

                    if (reservation == null)
                    {
                        _logger.LogWarning("Reservation not found for TxnRef: {TxnRef}", response.vnp_TxnRef);
                        return ApiResponse<object>.Fail("Reservation not found");
                    }

                    reservation.Status = BookingStatus.Confirmed;
                    reservation.UpdatedAt = DateTime.Now;
                    reservation.Room.IsAvailable = false;
                    reservation.Room.UpdatedAt = DateTime.Now;

                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Reservation {ReservationId} confirmed successfully", reservation.Id);

                    _taskQueue.QueueBackgroundWorkItem(async serviceProvider =>
                    {
                        var scopedUnitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
                        var logger = serviceProvider.GetRequiredService<ILogger<VnPayService>>();

                        try
                        {
                            var transaction = new Payment
                            {
                                ReservationId = reservation.Id,
                                Amount = reservation.TotalPrice,
                                Method = PaymentMethod.VNPay,
                                Status = PaymentStatus.Completed,
                                TransactionId = response.vnp_TransactionNo,
                                CreatedAt = DateTime.UtcNow
                            };

                            await scopedUnitOfWork.Payments.AddAsync(transaction);
                            await scopedUnitOfWork.SaveChangesAsync();

                            logger.LogInformation("Background Payment saved. ReservationId={ReservationId}", reservation.Id);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to save Payment in background.");
                        }
                    });

                    return ApiResponse<object>.Ok(new
                    {
                        Message = "Payment successful",
                        ReservationId = reservation.Id
                    });
                }
                else
                {
                    _logger.LogWarning("Payment failed - Code: {Code}, TxnRef: {TxnRef}", response.vnp_ResponseCode, response.vnp_TxnRef);
                    return ApiResponse<object>.Fail($"Payment failed with response code: {response.vnp_ResponseCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in HandlePaymentResponseAsync");
                return ApiResponse<object>.Fail("Internal server error");
            }
        }

        private string GetClientIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null || context.Connection == null)
                return "Unknown";

            var forwardedIp = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedIp))
            {
                return forwardedIp.Split(',').First().Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
