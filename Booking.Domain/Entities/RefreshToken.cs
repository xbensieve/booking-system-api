using System.ComponentModel.DataAnnotations.Schema;

namespace Booking.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string TokenHash { get; set; } = null!;
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
