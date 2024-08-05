using QuizWhiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Response
{
    public class QuizParticipantsDTO
    {
        public string QuizTitle { get; set; } = string.Empty;

        public string? Username { get; set; } = string.Empty;

        public int Rank { get; set; }

        public int Score { get; set; }

        public int Prize { get; set; }

        public int TotalParticipants { get; set; }

        public int TotalMarks { get; set; }

        public int? PersonalRank { get; set; } = 0;

        public int? PrizeMoney { get; set; } = 0;

        public int? AchievedScore { get; set; } = 0;

    }
}
