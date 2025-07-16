using System.ComponentModel.DataAnnotations;

namespace Booking.Application.DTOs.Hotel
{
    public class HotelImageRequest
    {
        [Required(ErrorMessage = "HotelId is required.")]
        public int HotelId { get; set; }

        [Required(ErrorMessage = "ImageUrl is required.")]
        [StringLength(1000, ErrorMessage = "ImageUrl must not exceed 1000 characters.")]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsMain { get; set; } = false;
    }
}
