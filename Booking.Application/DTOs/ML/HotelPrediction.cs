namespace Booking.Application.DTOs.ML
{
    public class HotelPrediction
    {
        public string HotelName { get; set; } = string.Empty;
        public float Score { get; set; }
    }
}
