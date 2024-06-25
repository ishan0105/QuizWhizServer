using System.Linq.Expressions;

namespace QuizWhiz.DataAccess.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        public Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        public Task<T?> GetLastOrDefaultOrderedByAsync<TProperty>(Expression<Func<T, bool>> predicate, Expression<Func<T, TProperty>> orderByExpression);

        public Task<bool> GetAnyAsync(Expression<Func<T, bool>> predicate);

        public List<T> GetAll();

        public IQueryable<T> GetTable();

        public Task CreateAsync(T entity);
    }
}