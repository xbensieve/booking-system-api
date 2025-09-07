using Booking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Booking.Repository.ApplicationContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<HotelImage> HotelImages { get; set; }
        public DbSet<RoomImage> RoomImages { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserOtp> UserOtps { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
