using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Domain.Entities
{
    public class Option
    {
        [Key]
        public int OptionId { get; set; }

        [Required]
        [ForeignKey("QuestionId")]
        public required int QuestionId { get; set; }

        [Required]
        public required int OptionNo { get; set; }

        [Required]
        public required string? OptionText { get; set; } = string.Empty;

        [Required]
        public bool IsAnswer { get; set; } = false;

        public Question Question { get; set; }

    }
}
