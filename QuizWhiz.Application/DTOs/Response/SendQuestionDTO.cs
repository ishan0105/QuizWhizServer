using QuizWhiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Response
{
    public class SendQuestionDTO
    {
        public Question Question { get; set; }
        public List<string> Options { get; set; }
    }
}
