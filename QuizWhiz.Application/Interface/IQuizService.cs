using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Application.DTOs.Response;

namespace QuizWhiz.Application.Interface
{
    public interface IQuizService
    {
        public Task<ResponseDTO> CreateNewQuizAsync(CreateQuizDTO quizDTO);

        public Task<ResponseDTO> GetQuizzesFilterAsync(GetQuizFilterDTO getQuizFilterDTO);

        public Task<ResponseDTO> AddQuizQuestionsAsync(QuizQuestionsDTO quizQuestionsDTO);

        public Task<ResponseDTO> GetQuizStatusCountAsync();

        public Task<ResponseDTO> GetQuizDifficultiesAsync();

        public Task<ResponseDTO> GetQuizCategoriesAsync();

        public Task<ResponseDTO> GetQuizDetailsAsync(string quizLink);
    }
}
