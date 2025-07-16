namespace Booking.Application.DTOs.Price
{
    public class PricingBreakdown
    {
        public int Nights { get; set; }
        public decimal BasePrice { get; set; }
        public decimal EarlyCheckInFee { get; set; }
        public decimal LateCheckOutFee { get; set; }
        public decimal Total => BasePrice + EarlyCheckInFee + LateCheckOutFee;
    }
}
