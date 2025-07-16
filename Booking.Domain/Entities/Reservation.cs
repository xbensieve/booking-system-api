using Booking.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking.Domain.Entities
{
    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "User UID is required.")]
        [StringLength(128, ErrorMessage = "User UID must not exceed 128 characters.")]
        public string UserUid { get; set; } = string.Empty;

        [Required(ErrorMessage = "Room ID is required.")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Check-in date is required.")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-out date is required.")]
        public DateTime CheckOutDate { get; set; }

        [Required(ErrorMessage = "Total price is required.")]
        [Range(0, 10_000_000, ErrorMessage = "Total price must be between 0 and 10,000,000 VND.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        [StringLength(1000, ErrorMessage = "Note cannot exceed 1000 characters.")]
        public string? Note { get; set; }
        public int NumberOfGuests { get; set; } = 1;
        [Range(0, 20, ErrorMessage = "Number of children must be between 0 and 20.")]
        public int NumberOfChildren { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EarlyCheckInSurcharge { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? LateCheckOutSurcharge { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualPrice { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [ForeignKey("UserUid")]
        public virtual User? User { get; set; }
        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
