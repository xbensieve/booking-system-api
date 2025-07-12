using System.ComponentModel.DataAnnotations;

namespace Booking.Service.Models
{
    public class ReservationRequest
    {
        [Required(ErrorMessage = "RoomId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "RoomId must be greater than 0.")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Check-in date is required.")]
        [DataType(DataType.DateTime)]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-out date is required.")]
        [DataType(DataType.DateTime)]
        public DateTime CheckOutDate { get; set; }

        [Required(ErrorMessage = "Number of guests is required.")]
        [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20.")]
        public int NumberOfGuests { get; set; }
    }
}
