using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Response
{
    public class GetCommentsDTO
    {
        public int QuizCommentId { get; set; }

        public required int QuizId { get; set; }

        public required int UserId { get; set; }

        public required string Comment { get; set; }

        public required DateTime CreatedDate { get; set; }

        public required string Username { get; set; }

        public required string NameAbbreviation { get; set; }

        public string? ProfileImageURL { get; set; }
    }
}
