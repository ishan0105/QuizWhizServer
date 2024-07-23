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
using Microsoft.Extensions.Configuration;
using System.Transactions;
using Microsoft.AspNetCore.Http;

namespace QuizWhiz.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtHelper _jwtHelper;
        private readonly HashingHelper _hashingHelper;
        private readonly EmailSenderHelper _emailSenderHelper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, JwtHelper jwtHelper, HashingHelper hashingHelper, EmailSenderHelper emailSenderHelper, IConfiguration configuration)
        {
            _jwtHelper = jwtHelper;
            _hashingHelper = hashingHelper;
            _emailSenderHelper = emailSenderHelper;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<ResponseDTO> LoginUserAsync(LoginUserDTO loginUserDTO)
        {
            bool isLoggedInEarlier = false;
            var user = (from u in _unitOfWork.UserRepository.GetTable()
                        join ur in _unitOfWork.UserRoleRepository.GetTable()
                        on u.RoleId equals ur.RoleId
                        where u.Email == loginUserDTO.Email && u.RoleId == 2
                        select new
                        {
                            u.UserId,
                            u.Username,
                            u.Email,
                            u.PasswordHash,
                            u.isLoggedInEarlier,
                            ur.RoleId,
                            ur.RoleName,
                        }).FirstOrDefault();

            User? user1 = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(u => u.Email == loginUserDTO.Email);

            if(user1 == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User not found",
                    Data = null,
                    StatusCode = HttpStatusCode.BadRequest
                };
            }


            if (user == null || !_hashingHelper.VerifyPassword(loginUserDTO.Password, user.PasswordHash))
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = "Invalid Email or Password!!",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            if(user1.isLoggedInEarlier == false)
            {
                user1.isLoggedInEarlier = true;
                await _unitOfWork.SaveAsync();
            }

            return new ResponseDTO()
            {
                IsSuccess = true,
                Message = "Logged In Successfully!!",
                Data = _jwtHelper.GenerateJwtToken(user.Email, user.RoleName, user.Username, user.isLoggedInEarlier),
                StatusCode = HttpStatusCode.OK
            };

        }

        public async Task<ResponseDTO> LoginAdminAsync(LoginUserDTO loginUserDTO)
        {
            var admin = (from u in _unitOfWork.UserRepository.GetTable()
                         join ur in _unitOfWork.UserRoleRepository.GetTable()
                         on u.RoleId equals ur.RoleId
                         where u.Email == loginUserDTO.Email && u.RoleId == 1
                         select new
                         {
                             u.UserId,
                             u.Username,
                             u.Email,
                             u.PasswordHash,
                             ur.RoleId,
                             ur.RoleName,
                         }).FirstOrDefault();

            if (admin == null || !_hashingHelper.VerifyPassword(loginUserDTO.Password, admin.PasswordHash))
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Invalid Credentials",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            return new()
            {
                IsSuccess = true,
                Message = "Admin Logged In Successfully!!",
                Data = _jwtHelper.GenerateJwtToken(admin.Email, admin.RoleName, admin.Username, true),
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

            if (userExists)
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
            var resetLink = $"{clientUrl}/reset-password/{token}";
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

        public async Task<ResponseDTO> GetProfileDetailsAsync(string username)
        {
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User does not Exists!!",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            ProfileDetailsDTO profileDetailsDTO = new()
            {
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Country = user.Country,
            };

            return new()
            {
                IsSuccess = true,
                Data = profileDetailsDTO,
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> SetProfileDetailsAsync(ProfileDetailsDTO profileDetailsDTO)
        {
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(u => u.Username == profileDetailsDTO.Username);
            if (user == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User does not Exists!!",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            user.FirstName = profileDetailsDTO.FirstName;
            user.LastName = profileDetailsDTO.LastName;
            user.PhoneNumber = profileDetailsDTO.PhoneNumber;
            user.Country = profileDetailsDTO.Country;
            user.ModifiedDate = DateTime.Now;
            await _unitOfWork.SaveAsync();

            return new()
            {
                IsSuccess = true,
                Message = "User updated successfully.",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> SetProfilePhoto(ProfileDetailsDTO profileDetailsDTO)
        {
            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(u => u.Username == profileDetailsDTO.Username);
            if (user == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User does not Exists!!",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            IFormFile file = profileDetailsDTO.ProfilePhoto;
            var filePath = "";
            if (file != null && file.Length > 0)
            {
                var directory = "D:\\QuizWhizClient\\quizwhiz-client\\public\\ProfilePhoto";
                var folderPath = Path.Combine(directory, profileDetailsDTO.Username.ToString());
                var extension = Path.GetExtension(file.FileName);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var FileName = profileDetailsDTO.Username + extension;
                filePath = Path.Combine(folderPath, FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }

            user.ProfileImageURL = filePath;
            await _unitOfWork.SaveAsync();

            return new()
            {
                IsSuccess = true,
                Message = "User updated successfully.",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> SetRecordSizeAsync(int recordSize)
        {
            _configuration["Records:Size"] = recordSize.ToString();
            return new()
            {
                IsSuccess = true,
                Data = _configuration["Records:Size"],
                Message = "Record Size Changed",
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}
