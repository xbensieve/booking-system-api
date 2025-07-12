using System.ComponentModel.DataAnnotations;

namespace Booking.Service.Models
{
    public class OrderInfo
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Reservation ID must be a positive integer.")]
        public int ReservationId { get; set; }
    }
}
