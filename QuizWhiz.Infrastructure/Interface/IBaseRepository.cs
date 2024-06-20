using System.Linq.Expressions;

namespace QuizWhiz.DataAccess.Interface
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        Task<T?> GetLastOrDefaultOrderedByAsync<TProperty>(Expression<Func<T, bool>> predicate, Expression<Func<T, TProperty>> orderByExpression);

        Task<bool?> GetAnyAsync(Expression<Func<T, bool>> predicate);

        Task CreateAsync(T entity);
    }
}
