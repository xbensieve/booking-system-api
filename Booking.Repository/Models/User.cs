using Booking.Repository.Enums;
using System.ComponentModel.DataAnnotations;

namespace Booking.Repository.Models
{
    public class User
    {
        [Key]
        [StringLength(128, ErrorMessage = "UID must not exceed 128 characters.")]
        public string Uid { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(256, ErrorMessage = "Email must not exceed 256 characters.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(256, ErrorMessage = "Name must not exceed 256 characters.")]
        public string? Name { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(20, ErrorMessage = "Phone number must not exceed 20 characters.")]
        public string? PhoneNumber { get; set; }
        [Required]
        public UserRole Role { get; set; } = UserRole.Customer;
        [StringLength(50)]
        public string? AuthProvider { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
