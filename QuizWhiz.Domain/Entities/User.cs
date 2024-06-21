using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Domain.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Username { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public required string Email { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Country { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "active";

        [MaxLength(10)]
        public string NameAbbreviation { get; set; } = string.Empty;

        [Required]
        public bool IsNotificationEnabled { get; set; } = false;

        [Required]
        [Column(TypeName = "timestamp without time zone")]
        public required DateTime CreatedDate { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? ModifiedDate { get; set; } 

        public string ResetToken { get; set; } = string.Empty;

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? ResetTokenExpiry { get; set; }

        [Required]
        public bool IsDeleted { get; set; } = false;

        public string ProfileImageURL { get; set; } = string.Empty;

        // Foreign key
        [ForeignKey("RoleId")]
        public int RoleId { get; set; } = 2;

        // Navigation properties
        public UserRole? Role { get; set; }
    }
}
