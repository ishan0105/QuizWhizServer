using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Response
{
    public class UserScoreboardDTO
    {
        public string? Username { get; set; }

        public int? Score { get; set; }

        public int? TotalScore { get; set; }

        public int? WinningAmount {  get; set; }

        public int? Rank { get; set; }
    }
}
