namespace Booking.Application.DTOs.Room
{
    public class RoomResponse
    {
        public int Id { get; set; }

        public int HotelId { get; set; }

        public string RoomNumber { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public decimal PricePerNight { get; set; }

        public int Capacity { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public List<RoomImageResponse>? Images { get; set; }
    }
}
