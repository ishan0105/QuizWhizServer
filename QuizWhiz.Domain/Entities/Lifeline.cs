using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Domain.Entities
{
    public class Lifeline
    {
        [Key]
        public required int LifelineId { get; set; }

        [Required]
        public required string Name { get; set; }
    }
}
