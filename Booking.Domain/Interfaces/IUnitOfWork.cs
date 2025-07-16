using Booking.Domain.Entities;

namespace Booking.Domain.Interfaces
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
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> SaveChangesAsync();
    }
}
