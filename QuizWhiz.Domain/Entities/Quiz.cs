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
       
        public int? ScheduleId { get; set; }

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
        public required string QuizStatus { get; set; }

        public int? WinningAmount { get; set; }

        [Required]
        public required string Difficulty { get; set; }

        [Required]
        public bool IsDeleted { get; set; } = false;

        [Required]
        public required int CreatedBy { get; set; }


        [Column(TypeName = "timestamp without time zone")]
        [Required]
        public required DateTime CreatedDate { get; set; }

        public string QuizLink { get; set; } = string.Empty;

        public int? ModifiedBy { get; set; }
     

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? ModifiedDate { get; set; }

        [Required]
        public bool IsPublished { get; set; } = false;

        public QuizCategory Category { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public User CreatedByUser { get; set; }


        [ForeignKey(nameof(ModifiedBy))]
        public User ModifiedByUser { get; set; }
    }
}
