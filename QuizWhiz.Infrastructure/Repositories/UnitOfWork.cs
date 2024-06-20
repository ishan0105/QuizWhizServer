using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuizWhiz.Domain.Entities;
using QuizWhiz.DataAccess.Interface;
using QuizWhiz.DataAccess.Data;
using QuizWhiz.DataAccess.Interface;

namespace QuizWhiz.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IBaseRepository<User>? UserRepository { get; set; }

        public IBaseRepository<UserRole>? UserRoleRepository { get; set; }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
