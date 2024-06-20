using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QuizWhiz.Domain.Entities;
using QuizWhiz.Application.Interfaces;
using QuizWhiz.DataAccess.Interfaces;
using Newtonsoft.Json.Linq;
using QuizWhiz.Application.DTOs.Response;
using System.Net;

namespace QuizWhiz.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtHelper _jwtHelper;
        private readonly HashingHelper _hashingHelper;
        private readonly EmailSenderHelper _emailSenderHelper;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUnitOfWork unitOfWork, JwtHelper jwtHelper, HashingHelper hashingHelper, EmailSenderHelper emailSenderHelper)
        {
            _jwtHelper = jwtHelper;
            _hashingHelper = hashingHelper;
            _emailSenderHelper = emailSenderHelper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> LoginUserAsync(LoginUserDTO loginUserDTO)
        {
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(u => u.Email == loginUserDTO.Email && u.RoleId == 2);

            if (user == null || !_hashingHelper.VerifyPassword(loginUserDTO.Password, user.PasswordHash))
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User not found",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            var role = await _unitOfWork.UserRoleRepository.GetFirstOrDefaultAsync(u => u.RoleId == user.RoleId);

            return new()
            {
                IsSuccess = true,
                Message = "User does exists.",
                Data = _jwtHelper.GenerateJwtToken(user.Email, role.RoleName, user.Username),
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> LoginAdminAsync(LoginUserDTO loginUserDTO)
        {
            var admin = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(u => u.Email == loginUserDTO.Email && u.RoleId == 1);

            if (admin == null || !_hashingHelper.VerifyPassword(loginUserDTO.Password, admin.PasswordHash))
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Admin not found",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            var role = await _unitOfWork.UserRoleRepository.GetFirstOrDefaultAsync(u => u.RoleId == admin.RoleId);

            return new()
            {
                IsSuccess = true,
                Message = "Admin does exists.",
                Data = _jwtHelper.GenerateJwtToken(admin.Email, role.RoleName, admin.Username),
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> SignUpUserAsync(SignUpUserDTO signUpUserDTO)
        {
            if (await _unitOfWork.UserRepository.GetAnyAsync(u => u.Email == signUpUserDTO.Email))
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Email is already registered.",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            string hashedPassword = _hashingHelper.HashPassword(signUpUserDTO.Password);

            User user = new User
            {
                Email = signUpUserDTO.Email,
                Username = signUpUserDTO.Username,
                PasswordHash = hashedPassword,
                NameAbbreviation = signUpUserDTO.Email.Substring(0, 2),
                CreatedDate = DateTime.Now,
            };

            await _unitOfWork.UserRepository.CreateAsync(user);
            await _unitOfWork.SaveAsync();

            return new()
            {
                IsSuccess = true,
                Message = "User registered successfully.",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> CheckUsernameAsync(CheckUsernameDTO checkUsernameDTO)
        {
            var userExists = await _unitOfWork.UserRepository.GetAnyAsync(u => u.Username == checkUsernameDTO.Username);

            if(userExists)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Username already exists!!",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
            else
            {
                return new()
                {
                    IsSuccess = true,
                    Message = "Username does not exists!!",
                    StatusCode = HttpStatusCode.OK
                };
            }
        }

        public async Task<ResponseDTO> SendPasswordResetLinkAsync(ForgotPasswordDTO forgotPasswordDTO)
        {
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(u => u.Email == forgotPasswordDTO.Email);
            if (user == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User not found",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.UserId}:{Guid.NewGuid()}"));
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.Now.AddHours(1);

            await _unitOfWork.SaveAsync();

            var clientUrl = "http://localhost:5173";
            var resetLink = $"{clientUrl}/reset-password?token={token}";
            var subject = "Reset Password";
            var body = $"Click here to reset your password: {resetLink}";
            var isSent = _emailSenderHelper.SendEmail(user.Email, subject, body);

            return new()
            {
                IsSuccess = true,
                Message = "Reset Link has been sent.",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> ValidateResetTokenAsync(string token)
        {
            
            var decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = decodedToken.Split(':');
            if (parts.Length != 2)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Invalid link",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            var userId = parts[0];
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(u => u.UserId == int.Parse(userId));
            if (user == null || user.ResetToken != token || user.ResetTokenExpiry < DateTime.Now)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Invalid or expired link",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            return new()
            {
                IsSuccess = true,
                Message = "Reset Link is Valid.",
                StatusCode = HttpStatusCode.OK
            };

        }

        public async Task<ResponseDTO> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO)
        {
            var decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(resetPasswordDTO.Token));
            var parts = decodedToken.Split(':');
            if (parts.Length != 2)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Invalid link",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            var userId = parts[0];
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(u => u.UserId == int.Parse(userId));
            if (user == null || user.ResetToken != resetPasswordDTO.Token || user.ResetTokenExpiry < DateTime.Now)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Invalid or expired link",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            var UpdatedPassword = _hashingHelper.HashPassword(resetPasswordDTO.NewPassword);

            user.PasswordHash = UpdatedPassword;
            user.ResetToken = String.Empty;
            user.ResetTokenExpiry = null;
            user.ModifiedDate = DateTime.Now;
            await _unitOfWork.SaveAsync();
            return new()
            {
                IsSuccess = true,
                Message = "User updated successfully.",
                StatusCode = HttpStatusCode.OK
            };
        }   
    }
}
