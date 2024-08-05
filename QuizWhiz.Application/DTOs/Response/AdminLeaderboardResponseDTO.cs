using QuizWhiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Response
{
    public class AdminLeaderboardResponseDTO
    {
        public PaginationDTO Pagination { get; set; } = new PaginationDTO();

        public List<ParticipantsDetailsDTO> QuizParticipants { get; set; } = new List<ParticipantsDetailsDTO>();
    }
}
