using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Domain.Entities
{
    public class QuizStatus
    {
        [Key]
        public int StatusId { get; set; }

        [Required]
        public required string StatusName { get; set; }
    }
}
