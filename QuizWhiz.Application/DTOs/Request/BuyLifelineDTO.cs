using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Request
{
    public class BuyLifelineDTO
    {

        [Required(ErrorMessage = "Username is required")]
        public required string UserName { get; set; } = String.Empty;

        [Required(ErrorMessage = "LifelineID is required")]
        public required int LifelineId { get; set; }

    }
}
