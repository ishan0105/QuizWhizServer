using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuizWhiz.Domain.Entities;
using QuizWhiz.DataAccess.Data;
using QuizWhiz.DataAccess.Interfaces;

namespace QuizWhiz.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            UserRepository = new BaseRepository<User>(_context);
            UserRoleRepository = new BaseRepository<UserRole>(_context);
            QuestionRepository = new BaseRepository<Question>(_context);
            QuestionTypeRepository = new BaseRepository<QuestionType>(_context);
            AnswerRepository = new BaseRepository<Answer>(_context);
            QuizRepository = new BaseRepository<Quiz>(_context);
            QuizCategoryRepository = new BaseRepository<QuizCategory>(_context);
            QuizScheduleRepository = new BaseRepository<QuizSchedule>(_context);
            QuizDifficultyRepository = new BaseRepository<QuizDifficulty>(_context);
            QuizStatusRepository = new BaseRepository<QuizStatus>(_context);
        }

        public IBaseRepository<User> UserRepository { get; set; }

        public IBaseRepository<UserRole> UserRoleRepository { get; set; }

        public IBaseRepository<Question> QuestionRepository { get; set; }

        public IBaseRepository<QuestionType> QuestionTypeRepository { get; set; }

        public IBaseRepository<Answer> AnswerRepository { get; set; }

        public IBaseRepository<Quiz> QuizRepository {  get; set; }

        public IBaseRepository<QuizCategory> QuizCategoryRepository { get; set; }

        public IBaseRepository<QuizSchedule> QuizScheduleRepository { get; set; }

        public IBaseRepository<QuizDifficulty> QuizDifficultyRepository { get; set; }

        public IBaseRepository<QuizStatus> QuizStatusRepository { get; set; }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
