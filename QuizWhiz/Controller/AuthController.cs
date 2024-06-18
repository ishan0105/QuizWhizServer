using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizWhiz.Infrastructure.Data;
using QuizWhiz.Application.DTOs;
using QuizWhiz.Domain.Interfaces;
using QuizWhiz.Application.Interfaces;
using System.Data;
using System.Linq;
using QuizWhiz.Application.DTOs.Request;

namespace QuizWhiz.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;
        private readonly IUserServices _userService;
        
        public AuthController(ApplicationDbContext context, IConfiguration configuration, IAuthService authService,IUserServices userServices)
        {
            _context = context;
            _configuration = configuration;
            _authService = authService;
            _userService = userServices;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO LoginCredential)
        {
            //change just for testing 

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var token = await _authService.AuthenticateUserAsync(LoginCredential);

            if (token == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            return Ok(new { token });

        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordDTO request)
        {
            try
            {
                var IsSuccess = _authService.SendPasswordResetLink(request.Email);
                if (IsSuccess)
                {
                    return Ok(new { message = "Password reset link has been sent." });
                }
                else
                {
                    return BadRequest(new { message = "Error in send link" });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("validate-reset-token")]
        public IActionResult ValidateResetToken([FromQuery] string token)
        {
            try
            {
                var isValid = _userService.ValidateResetToken(token);
                if (isValid)
                {
                    return Ok(new { Message = "Valid token" });
                }
                else
                {
                    return BadRequest(new { Message = "Invalid or expired token" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordDTO request)
        {
            try
            {
                var result = _userService.ResetPassword(request.Token, request.NewPassword);
                if (result)
                {
                    return Ok(new { Message = "Password reset successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to reset password" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
    }

}

