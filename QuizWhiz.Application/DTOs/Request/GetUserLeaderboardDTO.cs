using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Request
{
    public class GetUserLeaderboardDTO
    {
        [Required]
        public string QuizLink { get; set; } = string.Empty;

        public string? Username { get; set; } = string.Empty;

        public string? SearchedWord { get; set; } = string.Empty;

        [Required]
        public int CurrentPage { get; set; }

        [Required]
        public required int PageSize { get; set; }
    }
}
