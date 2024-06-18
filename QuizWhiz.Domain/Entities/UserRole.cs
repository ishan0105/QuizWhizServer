using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Domain.Entities
{
    public class UserRole
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(50)]
        public required string RoleName { get; set; }

        // Navigation properties
        public ICollection<User>? Users { get; set; }
    }
}
