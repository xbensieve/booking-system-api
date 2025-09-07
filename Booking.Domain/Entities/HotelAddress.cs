using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking.Domain.Entities
{
    public class HotelAddress
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AddressId { get; set; }

        [Required(ErrorMessage = "Street address is required.")]
        [StringLength(256, ErrorMessage = "Street address must not exceed 256 characters.")]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, ErrorMessage = "City must not exceed 100 characters.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(100, ErrorMessage = "Country must not exceed 100 characters.")]
        public string Country { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Postal code must not exceed 20 characters.")]
        public string? PostalCode { get; set; }
        [Required]
        public int HotelId { get; set; }
        [ForeignKey(nameof(HotelId))]
        public virtual Hotel Hotel { get; set; } = null!;
    }
}
