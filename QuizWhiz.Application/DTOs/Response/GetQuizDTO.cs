using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Response
{
    public class GetQuizDTO
    {
        public int QuizId { get; set; }

        public string? Title { get; set; } = null;

        public string? Description { get; set; } = null;

        public int? CategoryId { get; set; } = null;

        public int? DifficultyId { get; set; } = null;

        public DateTime ScheduledDate { get; set; }

        public string? QuizLink { get; set; } = null;
    }
}
