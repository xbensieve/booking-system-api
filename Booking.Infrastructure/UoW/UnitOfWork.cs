using Booking.Domain.Entities;
using Booking.Domain.Interfaces;
using Booking.Infrastructure.Repositories;
using Booking.Repository.ApplicationContext;
using Microsoft.EntityFrameworkCore.Storage;

namespace Booking.Infrastructure.UoW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction _transaction;
        public IGenericRepository<User> Users { get; }
        public IGenericRepository<Reservation> Reservations { get; }
        public IGenericRepository<Hotel> Hotels { get; }
        public IGenericRepository<Room> Rooms { get; }
        public IGenericRepository<HotelImage> HotelImages { get; }
        public IGenericRepository<RoomImage> RoomImages { get; }
        public IGenericRepository<Review> Reviews { get; }
        public IGenericRepository<Payment> Payments { get; }
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Users = new GenericRepository<User>(_context);
            Reservations = new GenericRepository<Reservation>(_context);
            Hotels = new GenericRepository<Hotel>(_context);
            Rooms = new GenericRepository<Room>(_context);
            HotelImages = new GenericRepository<HotelImage>(_context);
            RoomImages = new GenericRepository<RoomImage>(_context);
            Reviews = new GenericRepository<Review>(_context);
            Payments = new GenericRepository<Payment>(_context);
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _context.SaveChangesAsync();
            await _transaction?.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _transaction?.RollbackAsync();
        }
    }
}
