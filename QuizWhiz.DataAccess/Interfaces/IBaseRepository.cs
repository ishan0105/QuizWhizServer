using System.Linq.Expressions;

namespace QuizWhiz.DataAccess.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        public Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        public Task<T?> GetLastOrDefaultOrderedByAsync<TProperty>(Expression<Func<T, bool>> predicate, Expression<Func<T, TProperty>> orderByExpression);

        public Task<bool> GetAnyAsync(Expression<Func<T, bool>> predicate);

        public Task<List<T>> GetAll();

        public IQueryable<T> GetTable();

        public Task<List<T>> GetWhereAsync(Expression<Func<T, bool>> predicate);

        public Task CreateAsync(T entity);
        
        public Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    }
}