namespace Booking.Domain.Entities
{
    public class UserOtp
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }
    }
}
