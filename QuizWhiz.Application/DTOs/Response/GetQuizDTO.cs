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

        public string? CategoryName { get; set; } = null;

        public int? TotalQuestion {  get; set; } = 0;

        public int? MarksPerQuestion { get; set; } = 0;

        public int? NegativePerQuestion { get; set; } = 0;

        public int? TotalMarks { get; set; } = 0;

        public int? MinMarks { get; set; } = 0;

        public int? WinningAmount { get; set; } = 0;

        public int? DifficultyId { get; set; } = null;

        public DateTime ScheduledDate { get; set; }

        public string? QuizLink { get; set; } = null;
        public string Status { get; set; } = null;
    }
}
