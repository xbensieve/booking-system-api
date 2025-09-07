using Booking.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Booking.Domain.Entities
{
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(256, ErrorMessage = "Email must not exceed 256 characters.")]
        public string Email { get; set; } = string.Empty;
        [StringLength(256, ErrorMessage = "Name must not exceed 256 characters.")]
        public string? Name { get; set; }
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(20, ErrorMessage = "Phone number must not exceed 20 characters.")]
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        [Required]
        [StringLength(512, ErrorMessage = "Password hash must not exceed 512 characters.")]
        public string PasswordHash { get; set; } = string.Empty;
        [Required]
        public UserRole Role { get; set; } = UserRole.Customer;
        [Required]
        public bool IsActive { get; set; } = true;
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LastLoginAt { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public virtual ICollection<UserOtp> Otps { get; set; } = new List<UserOtp>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
