using System.Linq.Expressions;

namespace Booking.Repository.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdAsync(object id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        IQueryable<T> Query();
        Task<IEnumerable<T>> SearchAsync(Expression<Func<T, bool>> predicate);
    }
}
