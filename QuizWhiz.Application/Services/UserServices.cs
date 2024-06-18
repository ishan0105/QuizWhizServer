using Microsoft.AspNetCore.Mvc;
using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Application.Interfaces;
using QuizWhiz.Domain.Entities;
using QuizWhiz.Domain.Helpers;
using QuizWhiz.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuizWhiz.Application.Services
{
    public class UserServices : IUserServices
    {
        private readonly IUserRepository _userRepository;
        private readonly HashingHelper _hashingHelper;

        public UserServices(IUserRepository userRepository, HashingHelper hashingHelper)
        {
            _userRepository = userRepository;
            _hashingHelper = hashingHelper;
        }
        public async Task<IActionResult> Register(SignUpUserDTO newUser)
        {
            try
            {
                if (await _userRepository.IsEmailTaken(newUser.Email))
                {
                    return new ConflictObjectResult(new { message = "Email is already registered." });
                }

                string hashedPassword = _hashingHelper.HashPassword(newUser.Password);

                var user = new User
                {
                    Email = newUser.Email,
                    Username = newUser.Username,
                    PasswordHash = hashedPassword,
                    NameAbbreviation = newUser.Email.Substring(0, 2),
                    CreatedDate = DateTime.Now,
                };

                var registeredUser = await _userRepository.RegisterUser(user);

                

                // Example of further logic with contestant, if needed
              

                return new OkObjectResult(new { message = "Registration successful." });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { message = $"Internal server error: {ex.Message}" })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<bool> checkUserName(string userName)
        {
            try
            {
                var user = _userRepository.IsValidUserName(userName);
                return user.Result == null;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool ValidateResetToken(string token)
        {
            try
            {
                /*token = Regex.Replace(token, @"[^a-zA-Z0-9+/=]", string.Empty);

                // Check for padding issues
                if (token.Length % 4 != 0)
                {
                    int padding = 4 - (token.Length % 4);
                    token = token.PadRight(token.Length + padding, '=');
                }*/
                var decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = decodedToken.Split(':');
                if (parts.Length != 2)
                {
                    return false;
                }

                var userId = parts[0];
                var user = _userRepository.GetUserById(int.Parse(userId));
                if (user == null || user.ResetToken != token || user.ResetTokenExpiry < DateTime.Now)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool ResetPassword(string token, string newPassword)
        {
            try
            {
                var decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = decodedToken.Split(':');
                if (parts.Length != 2)
                {
                    throw new Exception("Invalid link");
                }

                var userId = parts[0];
                var user = _userRepository.GetUserById(int.Parse(userId));
                if (user == null || user.ResetToken != token || user.ResetTokenExpiry < DateTime.Now)
                {
                    throw new Exception("Invalid or expired link");
                }

               
                var UpdatedPassword = _hashingHelper.HashPassword(newPassword);

                user.PasswordHash = UpdatedPassword;
                user.ResetToken = String.Empty;
                user.ResetTokenExpiry = null;
                user.ModifiedDate  = DateTime.Now;
                _userRepository.UpdateUser(user);
                return true;
            }
            catch (Exception ex)
            {
                // Log exception (optional)
                throw new Exception("Failed to reset password", ex);
            }
        }

       

    }
}
