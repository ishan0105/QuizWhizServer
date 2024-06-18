using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Request
{
    public class SignUpUserDTO
    {
        [Required(ErrorMessage = "Username is required")]
        [RegularExpression("^[a-z][a-z0-9._-]{2,19}$", ErrorMessage = "Username must start with a lowercase letter, be between 3 and 20 characters, and can only contain lowercase letters, numbers, underscores, periods, and hyphens.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Please provide a valid Email")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [Column(TypeName = "character varying")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[a-z])(?=.*\W).{8,15}$", ErrorMessage = "Please provide a strong Password")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Column(TypeName = "character varying")]
        [Compare("Password")]
        public required string ConfirmPassword { get; set; }
    }
}
