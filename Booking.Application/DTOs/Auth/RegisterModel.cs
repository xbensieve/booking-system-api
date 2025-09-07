using System.ComponentModel.DataAnnotations;

namespace Booking.Application.DTOs.Auth
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256, ErrorMessage = "Email must be less than 256 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(256, ErrorMessage = "Full name must be less than 256 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, ErrorMessage = "Password must not exceed 128 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, ErrorMessage = "Phone number must not exceed 15 characters")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
