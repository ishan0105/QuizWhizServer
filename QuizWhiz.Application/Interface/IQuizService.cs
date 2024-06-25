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
        public Task<ResponseDTO> CreateNewQuizAsync(QuizDTO quizDTO);
    }
}
