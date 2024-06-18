using Microsoft.AspNetCore.Mvc;
using QuizWhiz.Application.DTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.Interfaces
{
    public interface IUserServices
    {
        Task<IActionResult> Register(SignUpUserDTO newUser);

        Task<bool> checkUserName(string userName);

        public bool ResetPassword(string token, string newPassword);

        public bool ValidateResetToken(string token);
       

    }
}
