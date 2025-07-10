using System.ComponentModel.DataAnnotations;

namespace Booking.Service.Models
{
    public class HotelRequest
    {
        [Required(ErrorMessage = "Hotel name is required.")]
        [StringLength(256, ErrorMessage = "Hotel name must not exceed 256 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(512, ErrorMessage = "Address must not exceed 512 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, ErrorMessage = "City must not exceed 100 characters.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(100, ErrorMessage = "Country must not exceed 100 characters.")]
        public string Country { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
        public string? Description { get; set; }

    }
}
