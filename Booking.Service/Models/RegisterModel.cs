using System.ComponentModel.DataAnnotations;

namespace Booking.Service.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "User ID is required")]
        [StringLength(128, ErrorMessage = "User ID must be less than 128 characters")]
        public string Uid { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256, ErrorMessage = "Email must be less than 256 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(256, ErrorMessage = "Full name must be less than 256 characters")]
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
}
