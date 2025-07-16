using System.ComponentModel.DataAnnotations;

namespace Booking.Application.DTOs.Room
{
    public class RoomSearchRequest
    {
        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Number of rooms must be greater than zero.")]
        public int NumberOfRooms { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Number of people must be greater than zero.")]
        public int NumberOfPeople { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int PageSize { get; set; } = 10;
    }
}
