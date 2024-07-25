using QuizWhiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Response
{
    public class CoinsAndLifelineCountDTO
    {
        public int CoinsCount { get; set; }

        public List<UserLifeLineDTO>? UserLifelines { get; set; }
        public List<Lifeline>? Lifelines { get; set; }

    }
}
