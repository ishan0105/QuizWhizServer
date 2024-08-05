using QuizWhiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Response
{
    public class QuizParticipantsDTO
    {
      public List<ParticipantsDetailsDTO>? participantsList {  get; set; }   
    }
}
