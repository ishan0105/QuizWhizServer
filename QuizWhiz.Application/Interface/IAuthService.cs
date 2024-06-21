using Microsoft.AspNetCore.Mvc;
using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.Interfaces
{
    public interface IAuthService
    {
        public Task<ResponseDTO> LoginUserAsync(LoginUserDTO loginUserDTO);

        public Task<ResponseDTO> LoginAdminAsync(LoginUserDTO loginUserDTO);

        public Task<ResponseDTO> SignUpUserAsync(SignUpUserDTO signUpUserDTO);

        public Task<ResponseDTO> CheckUsernameAsync(CheckUsernameDTO checkUsernameDTO);

        public Task<ResponseDTO> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO);

        public Task<ResponseDTO> ValidateResetTokenAsync(string token);

        public Task<ResponseDTO> SendPasswordResetLinkAsync(ForgotPasswordDTO forgotPasswordDTO);

        public Task<ResponseDTO> GetProfileDetailsAsync(string username);

        public Task<ResponseDTO> SetProfileDetailsAsync(ProfileDetailsDTO profileDetailsDTO);
    }
}
