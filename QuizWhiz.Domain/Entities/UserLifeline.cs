using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Domain.Entities
{
    public class UserLifeline
    {
        [Key]
        public int UserLifelineId { get; set; }

        [Required]
        [ForeignKey("UserId")]
        public required int UserId { get; set; }

        [Required]
        [ForeignKey("LifelineId")]
        public required int LifelineId { get; set; }

        [Required]
        public required int LifelineCount { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [ForeignKey(nameof(LifelineId))]
        public Lifeline? Lifeline { get; set; }
    }
}
