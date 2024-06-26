using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Request
{
    public class GetQuizFilterDTO
    {
        public required int StatusId { get; set; }

        public required int DifficultyId { get; set; }

        public required int CategoryId { get; set; }

        public required int CurrentPage { get; set; }
    }
}
