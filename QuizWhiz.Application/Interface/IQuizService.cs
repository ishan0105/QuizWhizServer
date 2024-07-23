using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Application.DTOs.Response;
using QuizWhiz.Domain.Entities;

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

        public Task<ResponseDTO> GetQuizQuestionsAsync(string quizLink);

        public Task<ResponseDTO> UpdateQuizDetailsAsync(UpdateQuizDetailsDTO updateQuizDetailsDTO);

        public Task<ResponseDTO> PublishQuizAsync(string quizLink);

        public Task<ResponseDTO> DeleteQuizAsync(string quizLink);

        public Task<ResponseDTO> UpdateQuizQuestionAsync(UpdateQuestionDetailsDTO updateQuestionDetailsDTO);

        public Task<ResponseDTO> DeleteQuizQuestionAsync(int questionId);

        public Task<ResponseDTO> GetSingleQuestion(GetSingleQuestionDTO getSingleQuestionDTO);

        public Task<ResponseDTO> GetCountOfQuestions(string quizLink);

        public Task<ResponseDTO> GetQuizTime(string QuizLink);

        public List<KeyValuePair<int, string>> GetActiveQuizzes();

        public Task<ResponseDTO> GetAllQuestions(string QuizLink);

        public Task<ResponseDTO> GetCorrectAnswer(int id);

        public Task<ResponseDTO> GetQuizWinners(string quizLink);

        public Task<ResponseDTO> UpdateScore(string quizLink, string userName, int QuestionId, List<int> userAnswers);

        public Task<ResponseDTO> RegisterUser(string quizLink, string userName);

        public Task<ResponseDTO> UpdateLeaderBoard(int QuizId);

        public Task<ResponseDTO> GetDisqualifiedUsers(string quizLink);

        public Task<ResponseDTO> GetQuiz(string quizLink);
    }
}