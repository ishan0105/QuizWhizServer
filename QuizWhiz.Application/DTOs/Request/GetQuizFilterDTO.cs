using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Request
{
    public class GetQuizFilterDTO
    {
        public string SearchValue { get; set; } = string.Empty;

        [Required]
        public required int StatusId { get; set; }

        [Required]
        public required int DifficultyId { get; set; }

        [Required]
        public required int CategoryId { get; set; }

        [Required]
        public required int CurrentPage { get; set; }

        public bool IsAscending { get; set; } = false;

        public string FilterBy { get; set; } = string.Empty;
    }
}
