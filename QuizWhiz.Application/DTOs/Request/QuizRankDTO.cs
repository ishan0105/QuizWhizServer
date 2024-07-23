using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Request
{
    public class QuizRankDTO
    {
        public required string QuizLink { get; set; } = string.Empty;

        public required string Username {  get; set; } = string.Empty;
    }
}
