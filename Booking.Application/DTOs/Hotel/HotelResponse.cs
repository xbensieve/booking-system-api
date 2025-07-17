using Booking.Application.DTOs.Room;

namespace Booking.Application.DTOs.Hotel
{
    public class HotelResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? Description { get; set; }

        public double AverageRating { get; set; }

        public int TotalReviews { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public List<RoomResponse>? Rooms { get; set; }

        public List<HotelImageResponse>? Images { get; set; }
    }
}
