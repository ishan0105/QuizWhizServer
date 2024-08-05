using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    
namespace QuizWhiz.Application.DTOs.Response
{
    public class ParticipantsDetailsDTO
    {
        public string username { get; set; }
        public int rank { get; set; }
        public int priceWon { get; set; }
        public int score { get; set; }
    }
}
