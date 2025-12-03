using System.Linq.Expressions;

namespace QuanLyVatTu_ASP.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(bool tracking = false);

        Task<T?> GetByIdAsync(int id, bool tracking = false);

        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            bool tracking = false);

        Task<IEnumerable<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            bool tracking = false);

        Task AddAsync(T entity);

        void Update(T entity);

        void Delete(T entity);
    }
}
