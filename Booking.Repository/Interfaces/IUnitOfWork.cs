using Booking.Repository.Models;

namespace Booking.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }
        IGenericRepository<Reservation> Reservations { get; }
        IGenericRepository<Hotel> Hotels { get; }
        IGenericRepository<Room> Rooms { get; }
        IGenericRepository<HotelImage> HotelImages { get; }
        IGenericRepository<RoomImage> RoomImages { get; }
        IGenericRepository<Review> Reviews { get; }
        IGenericRepository<Payment> Payments { get; }
        Task<int> SaveChangesAsync();
    }
}
