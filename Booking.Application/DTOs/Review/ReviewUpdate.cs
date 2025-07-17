using System.ComponentModel.DataAnnotations;

namespace Booking.Application.DTOs.Review
{
    public class ReviewUpdate
    {
        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment is required.")]
        [StringLength(2000, ErrorMessage = "Comment must not exceed 2000 characters.")]
        public string Comment { get; set; }
    }
}
