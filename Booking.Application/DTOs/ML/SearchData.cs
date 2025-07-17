namespace Booking.Application.DTOs.ML
{
    public class SearchData
    {
        public string Destination { get; set; }
        public float Budget { get; set; }
        public float NumPeople { get; set; }
        public float Rating { get; set; }
        public string HotelName { get; set; } = "";
    }
}
