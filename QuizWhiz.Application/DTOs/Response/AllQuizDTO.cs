using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Response
{
    public class AllQuizDTO
    {
        public List<GetQuizDTO>? GetQuizzes {  get; set; }

        public PaginationDTO? Pagination { get; set; }
    }
}
