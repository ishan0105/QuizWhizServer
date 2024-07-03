using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Request
{
    public class AddQuizCommentDTO
    {
        [Required]
        public required string QuizLink { get; set; }

        [Required]
        public required string Comment { get; set; }
    }
}
