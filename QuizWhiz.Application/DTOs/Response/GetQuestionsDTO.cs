using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Domain.Entities;

namespace QuizWhiz.Application.DTOs.Response
{
    public class GetQuestionsDTO
    {
        public int QuestionId { get; set; }

        public required int QuizId { get; set; }

        public required int QuestionTypeId { get; set; }

        public required string QuestionText { get; set; }

        public List<Option>? Options { get; set; }
    }
}
