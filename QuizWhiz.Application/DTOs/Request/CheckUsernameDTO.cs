using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Request
{
    public class CheckUsernameDTO
    {
        [Required(ErrorMessage = "Username is required")]
        [RegularExpression("^[a-z][a-z0-9._-]{2,19}$", ErrorMessage = "Username must start with a lowercase letter, be between 3 and 20 characters, and can only contain lowercase letters, numbers, underscores, periods, and hyphens.")]
        public required string Username { get; set; }
    }
}
