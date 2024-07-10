using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Request
{
    public class GetSingleQuestionDTO
    {
        [Required]
        public required string QuizLink { get; set; }

        [Required]
        public required int QuestionCount { get; set; }

        public List<int>? Answers { get; set; }
    }
}
