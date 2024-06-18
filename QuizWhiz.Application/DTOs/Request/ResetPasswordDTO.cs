using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Application.DTOs.Request
{
    public class ResetPasswordDTO
    {
        public required string Token { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [Column(TypeName = "character varying")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[a-z])(?=.*\W).{8,15}$", ErrorMessage = "Please enter a strong Password")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Column(TypeName = "character varying")]
        [Compare("NewPassword")]
        public required string ConfirmNewPassword { get; set; }
    }
}
