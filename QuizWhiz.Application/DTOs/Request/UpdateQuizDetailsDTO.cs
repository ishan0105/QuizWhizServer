using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Request
{
    public class UpdateQuizDetailsDTO
    {
        [Required]
        public required string QuizLink { get; set; }
            
        [Required(ErrorMessage = "Title is required")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public required int CategoryId { get; set; }

        [Required(ErrorMessage = "Difficulty is required")]
        public required int DifficultyId { get; set; }

        [Required(ErrorMessage = "Winning Amount is required")]
        public int? WinningAmount { get; set; } = 0;

        [Required(ErrorMessage = "Schedule Date is required")]
        public required DateTime ScheduleDate { get; set; }
    }
}
