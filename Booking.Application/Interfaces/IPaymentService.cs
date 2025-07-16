using Booking.Application.DTOs.Common;
using Booking.Application.DTOs.Order;
using Booking.Application.DTOs.Payment;

namespace Booking.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<ApiResponse<object>> CreatePaymentUrlAsync(OrderInfo order);
        Task<ApiResponse<object>> HandlePaymentResponseAsync(PaymentResponse response);
    }
}
