using Microsoft.AspNetCore.Mvc;
using QuizWhiz.Infrastructure.Data;
using QuizWhiz.Domain.Helpers;
using QuizWhiz.Application.Interfaces;
using QuizWhiz.Application.DTOs.Request;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QuizWhiz.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HashingHelper _hashingHelper;
        private readonly IUserServices  _userServices;
        public UserController(ApplicationDbContext context, HashingHelper hashingHelper, IUserServices userServices)
        {
            _context = context;
            _hashingHelper = hashingHelper;
            _userServices = userServices;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] SignUpUserDTO newUser)
        {           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _userServices.Register(newUser);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("checkUserName")]
        public async Task<bool> checkUserName(string userName)
        {
            var CheckuserName = await _userServices.checkUserName(userName);
            return CheckuserName;

        }
    }
}
