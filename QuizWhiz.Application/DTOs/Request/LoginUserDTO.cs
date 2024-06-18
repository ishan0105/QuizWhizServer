using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Request
{
    public class LoginUserDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Please provide a valid Email")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [Column(TypeName = "character varying")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[a-z])(?=.*\W).{8,15}$", ErrorMessage = "Please provide a strong Password")]
        public required string Password { get; set; }
    }
}
