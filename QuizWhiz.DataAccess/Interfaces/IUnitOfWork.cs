using QuizWhiz.Domain.Entities;

namespace QuizWhiz.DataAccess.Interfaces
{
    public interface IUnitOfWork
    {
        public IBaseRepository<User> UserRepository { get; set; }

        public IBaseRepository<UserRole> UserRoleRepository { get; set; }

        public IBaseRepository<Question> QuestionRepository { get; set; }

        public IBaseRepository<QuestionType> QuestionTypeRepository { get; set; }

        public IBaseRepository<Answer> AnswerRepository { get; set; }

        public IBaseRepository<Quiz> QuizRepository { get; set; }

        public IBaseRepository<QuizCategory> QuizCategoryRepository { get; set; }

        public IBaseRepository<QuizSchedule> QuizScheduleRepository { get; set; }

        public IBaseRepository<QuizDifficulty> QuizDifficultyRepository { get; set; }

        public IBaseRepository<QuizStatus> QuizStatusRepository {  get; set; }

        public IBaseRepository<QuizComments> QuizCommentsRepository { get; set; }

        public Task SaveAsync();
    }
}