using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http.HttpResults;

namespace QuizWhiz.Domain.Entities
{
    public class QuizComments
    {
        [Key]
        public int QuizCommentId { get; set; }

        [Required]
        [ForeignKey("QuizId")]
        public required int QuizId { get; set; }

        [Required]
        [ForeignKey("UserId")]
        public required int UserId { get; set; }

        [Required]
        public required string Comment { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        [Required]
        public required DateTime CreatedDate { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(QuizId))]
        public Quiz Quiz { get; set; }
    }
}
