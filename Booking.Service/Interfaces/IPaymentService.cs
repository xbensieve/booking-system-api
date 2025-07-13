using Booking.Service.Models;

namespace Booking.Service.Interfaces
{
    public interface IPaymentService
    {
        Task<ApiResponse<object>> CreatePaymentUrlAsync(OrderInfo order);
        Task<ApiResponse<object>> HandlePaymentResponseAsync(PaymentResponse response);
    }
}
