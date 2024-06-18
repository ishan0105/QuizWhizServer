using QuizWhiz.Application.DTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string> AuthenticateUserAsync(LoginUserDTO loginCredential);

        public bool SendPasswordResetLink(string email);
    }
}
