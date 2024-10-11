using System.Linq.Expressions;

namespace TalentSpot.Domain.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(Guid id);
        Task<T> GetById<T>(Guid id, bool eager = false) where T : class;
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
        Task DeleteRangeAsync(List<T> entities);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> List<T>(bool eager = false, IEnumerable<string> includes = null) where T : class;
        Task<IEnumerable<T>> List<T>(Expression<Func<T, bool>> predicate, bool eager = false, IEnumerable<string> includes = null) where T : class;
    }
}
