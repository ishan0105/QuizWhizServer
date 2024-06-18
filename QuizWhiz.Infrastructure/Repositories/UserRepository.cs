using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizWhiz.Domain.Interfaces;
using QuizWhiz.Infrastructure.Data;
using QuizWhiz.Domain.Entities;
using QuizWhiz.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        
       
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> IsValidUserName(string userName)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(r => r.Username == userName);
                return user;
            }
            catch (Exception exp)
            {
                return null;
            }
        }
        public User GetUserByEmail(string email)
        {
            try
            {
                var user =  _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Email == email)                  
                    .FirstOrDefault();

                if(user != null ) 
                {
                    return user;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex);
                return null;
            }

        }
        public async Task<bool> IsEmailTaken(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User> RegisterUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        public void UpdateUser(User user)
        {
            try
            {
                _context.Users.Update(user);
                _context.SaveChanges();
            }
            catch(Exception ex) 
            {
                throw new Exception("Failed to save", ex);
            }
            
        }
       public User GetUserById(int id)
        {
            try
            {
                var user = _context.Users     
                    .Where(u => u.UserId == id)
                    .FirstOrDefault();

                if (user != null)
                {
                    return user;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }

        }
    }
}
