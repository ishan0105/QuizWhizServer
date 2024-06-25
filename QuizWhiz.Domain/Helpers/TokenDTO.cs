using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Response
{
    public class TokenDTO
    {
        public string? UserRole {  get; set; }

        public string? Username { get; set; }

        public string? Email { get; set; }
    }
}
