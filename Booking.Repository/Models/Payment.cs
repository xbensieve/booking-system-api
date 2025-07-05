using Booking.Repository.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking.Repository.Models
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Booking ID is required.")]
        public int ReservationId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0, 10_000_000, ErrorMessage = "Amount must be between 0 and 10,000,000 VND.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        [Required]
        public PaymentStatus Status { get; set; }

        [StringLength(256, ErrorMessage = "Transaction ID must not exceed 256 characters.")]
        public string? TransactionId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ReservationId")]
        public virtual Reservation? Reservation { get; set; }
    }
}
