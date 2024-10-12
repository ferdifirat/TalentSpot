using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using TalentSpot.Domain.Interfaces;
using TalentSpot.Infrastructure.Data;

namespace TalentSpot.Infrastructure.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> GetById<T>(Guid id, bool eager = false) where T : class
        {
            var entity = await _context.Set<T>()
                    .FindAsync(id);

            if (entity == null)
                return entity;

            var navigations = new List<Task>();
            if (eager)
                foreach (var property in _context.Model.FindEntityType(typeof(T)).GetNavigations())
                {
                    PropertyInfo propertyInfo = typeof(T).GetProperty(property.Name);
                    var jsonIgnore = propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>();
                    if (jsonIgnore == null)
                        navigations.Add(_context.Entry(entity).Navigation(property.Name).LoadAsync());
                }

            Task.WaitAll(navigations.ToArray());
            _context.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        public async Task DeleteRangeAsync(List<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public async Task<IEnumerable<T>> List<T>(bool eager = false, IEnumerable<string> includes = null) where T : class
        {
            var query = QueryWithInclude<T>(eager);

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            var result = await Task.Run(() =>
                query.AsEnumerable()
                );
            return result;
        }

        public async Task<IEnumerable<T>> List<T>(Expression<Func<T, bool>> predicate, bool eager = false, IEnumerable<string> includes = null) where T : class
        {
            var query = QueryWithInclude<T>(eager);

            if (predicate != null)
                query = query.Where(predicate);

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            var result = await Task.Run(() =>
                    query.AsEnumerable()
                 );
            return result;
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        internal virtual IQueryable<T> QueryWithInclude<T>(bool eager = false) where T : class
        {
            var query = _context.Set<T>().AsQueryable().AsNoTracking();
            if (eager)
                foreach (var property in _context.Model.FindEntityType(typeof(T)).GetNavigations())
                {
                    PropertyInfo propertyInfo = typeof(T).GetProperty(property.Name);
                    var jsonIgnore = propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>();
                    if (jsonIgnore == null)
                        query = query.Include(property.Name);
                }
            return query;
        }
    }
}