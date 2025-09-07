using System.ComponentModel.DataAnnotations;

namespace Booking.Application.DTOs.Hotel
{
    public class HotelRequest
    {
        [Required(ErrorMessage = "Hotel name is required.")]
        [StringLength(256, ErrorMessage = "Hotel name must not exceed 256 characters.")]
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        [StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
        public string? Description { get; set; }
        [Required(ErrorMessage = "Address is required.")]
        public List<HotelAddressRequest> Addresses { get; set; } = new List<HotelAddressRequest>();
    }
}
