using QuizWhiz.Domain.Entities;

namespace QuizWhiz.DataAccess.Interfaces
{
    public interface IUnitOfWork
    {
        public IBaseRepository<User> UserRepository { get; set; }

        public IBaseRepository<UserRole> UserRoleRepository { get; set; }

        public Task SaveAsync();
    }
}