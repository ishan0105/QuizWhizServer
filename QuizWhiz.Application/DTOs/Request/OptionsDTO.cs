using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Request
{
    public class OptionsDTO
    {
        public required string OptionText { get; set; }

        public required bool IsAnswer { get; set; } = false;
    }
}
