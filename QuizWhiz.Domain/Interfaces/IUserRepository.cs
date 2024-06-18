using Microsoft.EntityFrameworkCore;
using QuizWhiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Domain.Interfaces
{
    public interface IUserRepository
    {
        User GetUserByEmail(string email);

        User GetUserById(int id);

        Task<bool> IsEmailTaken(string email);

        Task<User> RegisterUser(User user);

        Task<User> IsValidUserName(string userName);

        public void UpdateUser(User user);
    }
}
