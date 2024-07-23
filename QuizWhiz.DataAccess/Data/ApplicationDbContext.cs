using Microsoft.EntityFrameworkCore;
using QuizWhiz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<Quiz> Quizzes { get; set; }

        public DbSet<QuizCategory> QuizCategories { get; set; }

        public DbSet<Question> Questions { get; set; }

        public DbSet<Option> Options { get; set; }

        public DbSet<QuestionType> QuestionTypes { get; set; }

        public DbSet<QuizDifficulty> QuizDifficulties { get; set; }
        
        public DbSet<QuizStatus> QuizStatuses { get; set; }

        public DbSet<QuizParticipants> QuizParticipants { get; set; }

        public DbSet<UserLifeline> UserLifeline { get; set; }

        public DbSet<UserCoins> UserCoins { get; set; }

        public DbSet<Lifeline> Lifeline { get; set; }


    }
}
