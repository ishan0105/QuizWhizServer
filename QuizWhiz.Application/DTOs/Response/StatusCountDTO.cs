using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Response
{
    public class StatusCountDTO
    {
        public required int PendingCount { get; set; } = 0;

        public required int UpcomingCount { get; set; } = 0;

        public required int ActiveCount { get; set; } = 0;

        public required int CompletedCount { get; set; } = 0;
    }
}
