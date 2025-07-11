using Booking.Repository.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking.Service.Models
{
    public class RoomRequest
    {
        [Required(ErrorMessage = "Room number is required.")]
        [StringLength(50, ErrorMessage = "Room number cannot exceed 50 characters.")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Room type is required.")]
        public RoomType Type { get; set; }

        [Required(ErrorMessage = "Price per night is required.")]
        [Range(100_000, 100_000_000, ErrorMessage = "Price must be between 100,000 VND and 100,000,000 VND.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerNight { get; set; }

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, 100, ErrorMessage = "Capacity must be between 1 and 100 people.")]
        public int Capacity { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }
    }
}
