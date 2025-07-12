namespace Booking.Service.Models
{
    public class ReservationResponse
    {
        public int Id { get; set; }
        public string UserUid { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string? Note { get; set; }
        public int NumberOfGuests { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public UserResponse? User { get; set; }
        public RoomResponse? Room { get; set; }
    }
}
