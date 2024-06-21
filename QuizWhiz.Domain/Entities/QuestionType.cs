using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Domain.Entities
{
    public class QuestionType
    {
        [Key]
        public int QuestionTypeId { get; set; }

        [Required]
        public required string QuesionTypeName { get; set; }
    }
}
