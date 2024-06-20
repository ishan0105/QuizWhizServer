using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizWhiz.Application.DTOs;
using System.Data;
using System.Linq;
using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Application.DTOs.Response;
using System.Net;
using QuizWhiz.Application.Services;
using QuizWhiz.DataAccess.Data;
using QuizWhiz.Application.Interfaces;

namespace QuizWhiz.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;

        public AuthController(ApplicationDbContext context, IConfiguration configuration, IAuthService authService)
        {
            _context = context;
            _configuration = configuration;
            _authService = authService;
        }

        [HttpPost("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ResponseDTO> LoginUser([FromBody] LoginUserDTO loginUserDTO)
        {
            if (!ModelState.IsValid)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Something Went Wrong",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
            return await _authService.LoginUserAsync(loginUserDTO);
        }

        [HttpPost("admin-login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ResponseDTO> LoginAdmin([FromBody] LoginUserDTO loginUserDTO)
        {
            if (!ModelState.IsValid)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Something Went Wrong",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
            return await _authService.LoginAdminAsync(loginUserDTO);
        }

        [HttpPost("sign-up")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<ResponseDTO> SignUpUser([FromBody] SignUpUserDTO newUser)
        {
            if (!ModelState.IsValid)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Something Went Wrong",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
            return await _authService.SignUpUserAsync(newUser);
        }

        [HttpPost("check-username")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<ResponseDTO> CheckUsername([FromBody] CheckUsernameDTO checkUsernameDTO)
        {
            if (!ModelState.IsValid)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Something Went Wrong",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
            return await _authService.CheckUsernameAsync(checkUsernameDTO);
        }

        [HttpPost("forgot-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ResponseDTO> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDTO)
        {
            if (!ModelState.IsValid)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Something Went Wrong",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
            return await _authService.SendPasswordResetLinkAsync(forgotPasswordDTO);
        }

        [HttpGet("validate-reset-password-token")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ResponseDTO> ValidateResetPasswordToken([FromQuery] string token)
        {
            if (!ModelState.IsValid)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Something Went Wrong",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
            return await _authService.ValidateResetTokenAsync(token);
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ResponseDTO> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
        {
            if (!ModelState.IsValid)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Something Went Wrong",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
            return await _authService.ResetPasswordAsync(resetPasswordDTO);
        }
    }
}
