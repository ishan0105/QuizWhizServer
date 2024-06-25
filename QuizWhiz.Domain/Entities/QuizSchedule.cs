using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Domain.Entities
{
    public class QuizSchedule
    {
        [Key]
        public int ScheduleId { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        [Required]
        public required DateTime ScheduledDate { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        [Required]
        public required DateTime CreatedDate { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? ModifiedDate { get; set; }
    }
}
