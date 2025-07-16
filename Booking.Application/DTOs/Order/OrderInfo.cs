using System.ComponentModel.DataAnnotations;

namespace Booking.Application.DTOs.Order
{
    public class OrderInfo
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Reservation ID must be a positive integer.")]
        public int ReservationId { get; set; }
    }
}
