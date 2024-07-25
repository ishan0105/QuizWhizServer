using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Domain.Entities
{
    public class UserCoins
    {
        [Key]
        public int UserCoinsId { get; set; }

        [Required]
        [ForeignKey("UserId")]
        public required int UserId { get; set; }

        [Required]
        public required int NoOfCoins { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
       
    }
}
