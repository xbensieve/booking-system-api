using System.ComponentModel.DataAnnotations;

namespace Booking.Application.DTOs.User
{
    public class UpdateUserRequest
    {
        [Required]
        [StringLength(256)]
        public string Name { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public string? AvatarUrl { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
