using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Request
{
    public class QuizQuestionsDTO
    {
        public int QuizId { get; set; }

        public required string QuizLink { get; set; }

        public List<QuestionDTO> QuestionDTOs { get; set; } = [];
    }
}
