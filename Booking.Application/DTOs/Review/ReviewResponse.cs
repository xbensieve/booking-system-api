namespace Booking.Application.DTOs.Review
{
    public class ReviewResponse
    {
        public int Id { get; set; }

        public int HotelId { get; set; }

        public string HotelName { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
