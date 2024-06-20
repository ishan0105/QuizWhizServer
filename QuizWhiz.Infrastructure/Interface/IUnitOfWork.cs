using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuizWhiz.Domain.Entities;

namespace QuizWhiz.DataAccess.Interface
{
    public interface IUnitOfWork
    {
        public IBaseRepository<User> UserRepository { get; set; }

        public IBaseRepository<UserRole>? UserRoleRepository { get; set; }

        Task SaveAsync();
    }
}
