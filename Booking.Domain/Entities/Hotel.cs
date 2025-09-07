using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking.Domain.Entities
{
    public class Hotel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Hotel name is required.")]
        [StringLength(256, ErrorMessage = "Hotel name must not exceed 256 characters.")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
        public string Description { get; set; } = string.Empty;
        [Range(0, 5)]
        public double AverageRating { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(15, ErrorMessage = "Phone number must not exceed 15 characters.")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public bool IsFeatured { get; set; } = false;
        public virtual ICollection<HotelAddress> Addresses { get; set; } = null!;
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
        public virtual ICollection<HotelImage> Images { get; set; } = new List<HotelImage>();
    }
}
