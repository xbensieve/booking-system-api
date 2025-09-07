using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking.Domain.Entities
{
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Hotel ID is required.")]
        public int HotelId { get; set; }

        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [StringLength(2000, ErrorMessage = "Comment must not exceed 2000 characters.")]
        public string? Comment { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("HotelId")]
        public virtual Hotel? Hotel { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
