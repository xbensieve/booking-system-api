namespace Booking.Application.DTOs.Hotel
{
    public class HotelAddressResponse
    {
        public int AddressId { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
    }
}
