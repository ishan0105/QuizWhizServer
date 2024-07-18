using QuizWhiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Response
{
    public class QuizWinners
    {
        public List<QuizParticipants> firstRankedUsers { get; set; }
        public List<QuizParticipants> secondRankedUsers { get; set; }
        public List<QuizParticipants> thirdRankedUsers { get; set; }
    }
}
