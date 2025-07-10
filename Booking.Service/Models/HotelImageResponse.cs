namespace Booking.Service.Models
{
    public class HotelImageResponse
    {
        public int Id { get; set; }

        public int HotelId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsMain { get; set; }
    }
}
