using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Domain.Entities
{
    public class Quiz
    {
        [Key]
        public int QuizId { get; set; }

        [Required]
        public required string Title { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        [ForeignKey("CategoryId")]
        public required int CategoryId { get; set; }

        [Required]
        [Column(TypeName = "timestamp without time zone")]
        public required DateTime ScheduledDate { get; set; }

        [Required]
        public required int TotalQuestion { get; set; }

        [Required]
        public required int MarksPerQuestion { get; set; }

        [Required]
        public required int NegativePerQuestion { get; set; }

        [Required]
        public required int TotalMarks { get; set; }

        [Required]
        public required int MinMarks { get; set; }

        [Required]
        [ForeignKey("StatusId")]
        public required int StatusId { get; set; }

        [Required]
        public int? WinningAmount { get; set; } = 0;

        [Required]
        [ForeignKey("DifficultyId")]
        public required int DifficultyId { get; set; }

        [Required]
        public bool IsDeleted { get; set; } = false;

        [Required]
        public required int CreatedBy { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        [Required]
        public required DateTime CreatedDate { get; set; }

        public string? QuizLink { get; set; } = string.Empty;

        public int? ModifiedBy { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? ModifiedDate { get; set; }

        public QuizCategory Category { get; set; }

        public QuizDifficulty Difficulty { get; set; }

        public QuizStatus Status { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public User CreatedByUser { get; set; }

        [ForeignKey(nameof(ModifiedBy))]
        public User ModifiedByUser { get; set; }
    }
}
