using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Request
{
    public class QuestionDTO
    {
        public int? QuizId { get; set; }

        public required int QuestionTypeId { get; set; }

        public required string QuestionText { get; set; }

        public List<OptionsDTO> Options { get; set; } = [];

        public bool IsTrue { get; set; } = false;
    }
}