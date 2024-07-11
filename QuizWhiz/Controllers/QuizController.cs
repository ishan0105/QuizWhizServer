using System.Net;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Application.DTOs.Response;
using QuizWhiz.Application.Interface;
using QuizWhiz.Application.Interfaces;
using QuizWhiz.DataAccess.Data;
using QuizWhiz.Domain.Helpers;

namespace QuizWhiz.API.Controllers
{
    [Route("api/quiz")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IQuizService _quizService;

        public QuizController(ApplicationDbContext context, IConfiguration configuration, IQuizService quizService)
        {
            _context = context;
            _configuration = configuration;
            _quizService = quizService;
        }

        [HttpPost("create-new-quiz")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [CustomAuthorize("Admin")]
        public async Task<ResponseDTO> CreateNewQuiz([FromBody] CreateQuizDTO quizDTO)
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

            return await _quizService.CreateNewQuizAsync(quizDTO);
        }

        [HttpPost("get-quizzes-filter")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        //[CustomAuthorize("Admin")]
        public async Task<ResponseDTO> GetQuizzesFilter([FromBody] GetQuizFilterDTO getQuizFilterDTO)
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

            return await _quizService.GetQuizzesFilterAsync(getQuizFilterDTO);
        }

        [HttpPost("add-quiz-questions")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ResponseDTO> AddQuizQuestions([FromBody] QuizQuestionsDTO quizQuestionsDTO)
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

            return await _quizService.AddQuizQuestionsAsync(quizQuestionsDTO);
        }

        [HttpGet("get-quiz-status-count")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ResponseDTO> GetQuizStatusCount()
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

            return await _quizService.GetQuizStatusCountAsync();
        }

        [HttpGet("get-quiz-difficulties")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ResponseDTO> GetQuizDifficulties()
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

            return await _quizService.GetQuizDifficultiesAsync();
        }

        [HttpGet("get-quiz-categories")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ResponseDTO> GetQuizCategories()
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

            return await _quizService.GetQuizCategoriesAsync();
        }

        [HttpGet("get-quiz-details")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ResponseDTO> GetQuizDetails([FromQuery] string quizLink)
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

            return await _quizService.GetQuizDetailsAsync(quizLink);
        }

        [HttpGet("get-quiz-questions")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ResponseDTO> GetQuizQuestions([FromQuery] string quizLink)
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

            return await _quizService.GetQuizQuestionsAsync(quizLink);
        }

        [HttpPost("update-quiz-details")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [CustomAuthorize("Admin")]
        public async Task<ResponseDTO> UpdateQuizDetails([FromBody] UpdateQuizDetailsDTO updateQuizDetailsDTO)
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

            return await _quizService.UpdateQuizDetailsAsync(updateQuizDetailsDTO);
        }

        [HttpGet("delete-quiz")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [CustomAuthorize("Admin")]
        public async Task<ResponseDTO> DeleteQuiz([FromQuery] string quizLink)
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

            return await _quizService.DeleteQuizAsync(quizLink);
        }

        [HttpPost("update-quiz-question")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [CustomAuthorize("Admin")]
        public async Task<ResponseDTO> UpdateQuizQuestion([FromBody] UpdateQuestionDetailsDTO updateQuestionDetailsDTO)
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

            return await _quizService.UpdateQuizQuestionAsync(updateQuestionDetailsDTO);
        }

        [HttpGet("delete-quiz-question")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [CustomAuthorize("Admin")]
        public async Task<ResponseDTO> DeleteQuizQuestion([FromQuery] int questionId)
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

            return await _quizService.DeleteQuizQuestionAsync(questionId);
        }

        [HttpPost("get-single-quiz-question")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ResponseDTO> GetSingleQuestion([FromBody] GetSingleQuestionDTO getSingleQuestionDTO)
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
            return await _quizService.GetSingleQuestion(getSingleQuestionDTO);
        }

        [HttpGet("get-count-of-questions")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ResponseDTO> GetCountOfQuestions([FromQuery] string quizLink)
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
            return await _quizService.GetCountOfQuestions(quizLink);
        }
        
        
        
    }
}
