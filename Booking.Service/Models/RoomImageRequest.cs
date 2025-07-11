using System.ComponentModel.DataAnnotations;

namespace Booking.Service.Models
{
    public class RoomImageRequest
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        [StringLength(1000)]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsMain { get; set; } = false;
    }
}
