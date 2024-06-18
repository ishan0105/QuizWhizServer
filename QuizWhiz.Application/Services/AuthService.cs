using QuizWhiz.Application.Interfaces;
using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Domain.Helpers;
using QuizWhiz.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtHelper _jwtHelper;
        private readonly HashingHelper _hashingHelper;
        private readonly EmailSenderHelper _emailSenderHelper;

        public AuthService(IUserRepository userRepository, JwtHelper jwtHelper, HashingHelper hashingHelper, EmailSenderHelper emailSenderHelper)
        {
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
            _hashingHelper = hashingHelper;
            _emailSenderHelper = emailSenderHelper;
        }

        public async Task<string> AuthenticateUserAsync(LoginUserDTO loginCredential)
        {
            var user = _userRepository.GetUserByEmail(loginCredential.Email);

            if (user == null || !_hashingHelper.VerifyPassword(loginCredential.Password, user.PasswordHash))
            {
                return null;
            }

            return _jwtHelper.GenerateJwtToken(user.Email, user.PasswordHash,user.Role.RoleName, user.Username);
        }

        public bool SendPasswordResetLink(string email)
        {
            var user = _userRepository.GetUserByEmail(email); 
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Create a token with user ID and a unique identifier
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.UserId}:{Guid.NewGuid()}"));
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.Now.AddHours(1);

            _userRepository.UpdateUser(user);

            var clientUrl = "http://localhost:5173";
            // Send email
            var resetLink = $"{clientUrl}/reset-password?token={token}";
            var subject = "Reset Password";
            var body = $"Click here to reset your password: {resetLink}";
            var isSent = _emailSenderHelper.SendEmail(user.Email, subject, body);
            return isSent;
        }
    }
}
