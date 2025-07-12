namespace Booking.Service.Models
{
    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Message { get; set; }
    }
}
