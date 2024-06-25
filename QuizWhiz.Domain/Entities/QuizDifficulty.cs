using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Domain.Entities
{
    public class QuizDifficulty
    {
        [Key]
        public int DifficultyId { get; set; }

        [Required]
        public required string DifficultyName { get; set; }
    }
}
