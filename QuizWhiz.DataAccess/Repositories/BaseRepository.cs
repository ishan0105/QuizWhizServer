using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuizWhiz.DataAccess.Data;
using QuizWhiz.DataAccess.Interfaces;

namespace QuizWhiz.DataAccess.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private DbSet<T> _dbSet;

        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<T?> GetLastOrDefaultOrderedByAsync<TProperty>(Expression<Func<T, bool>> predicate, Expression<Func<T, TProperty>> orderByExpression)
        {
            return await _dbSet.Where(predicate)
                .OrderBy(orderByExpression)
                .LastOrDefaultAsync();
        }

        public IQueryable<T> GetTable()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<bool> GetAnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<List<T>> GetAll()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<List<T>> GetWhereAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate)
                .ToListAsync();
        }

        public async Task CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        public async Task<List<T>> GetWhereOrderByAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> orderByExpression)
        {
            return await _dbSet.Where(predicate).OrderBy(orderByExpression).ToListAsync();
        }
    }
}
