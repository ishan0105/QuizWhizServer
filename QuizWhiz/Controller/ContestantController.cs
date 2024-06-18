using Microsoft.AspNetCore.Mvc;
using QuizWhiz.Domain.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QuizWhiz.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContestantController : ControllerBase
    {
        [HttpGet("testget")]
        [CustomAuthorize("Contestant")]
        public IActionResult GetName()
        {
            return Ok(new {message="hello from contestant controller"});
        }
    }
}
