using Microsoft.EntityFrameworkCore;
using QuanLyVatTu_ASP.DataAccess;
using System.Linq.Expressions;

namespace QuanLyVatTu_ASP.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // Lấy tất cả (hạn chế dùng)
        public async Task<IEnumerable<T>> GetAllAsync(bool tracking = false)
        {
            IQueryable<T> query = _dbSet;

            if (!tracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }

        // Lấy theo ID
        public async Task<T?> GetByIdAsync(int id, bool tracking = false)
        {
            var entity = await _dbSet.FindAsync(id);

            if (entity != null && !tracking)
                _context.Entry(entity).State = EntityState.Detached;

            return entity;
        }

        // Truy vấn có điều kiện
        public async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            bool tracking = false)
        {
            IQueryable<T> query = _dbSet.Where(predicate);

            if (!tracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }

        // Phân trang
        public async Task<IEnumerable<T>> GetPagedAsync(
            int pageNumber, int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            bool tracking = false)
        {
            IQueryable<T> query = _dbSet;

            if (predicate != null)
                query = query.Where(predicate);

            if (!tracking)
                query = query.AsNoTracking();

            return await query.Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }
    }
}
