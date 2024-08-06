using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Domain.Entities
{
    public class QuizParticipants
    { 
        [Key]
        public int QuizParticipantId { get; set; }

        [Required]
        [ForeignKey("QuizId")]
        public int QuizId { get; set; }

        [Required]
        [ForeignKey("UserId")]
        public int UserId { get; set; }

        public int CorrectQuestions { get; set; } = 0;

        public int TotalScore { get; set; } = 0;

        public int WinningAmount { get; set; } = 0;

        public int Rank { get; set; } = 0;

        public bool IsDisqualified { get; set; } = false;

        public bool IsHeartUsed { get; set; } = false;

        public bool IsSkipUsed { get; set; } = false;

        public bool IsFiftyUsed { get; set; } = false;

        [ForeignKey(nameof(QuizId))]
        public required Quiz Quiz { get; set; }

        [ForeignKey(nameof(UserId))]
        public required User User { get; set; }

        
    }
}
