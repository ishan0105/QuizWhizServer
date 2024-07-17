using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Application.DTOs.Response;
using System.Transactions;
using QuizWhiz.Application.Interface;
using QuizWhiz.Domain.Entities;
using Microsoft.Extensions.Configuration;
using QuizWhiz.DataAccess.Interfaces;
using QuizWhiz.Domain.Helpers;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Buffers;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;

namespace QuizWhiz.Application.Services
{
    public class QuizService : IQuizService
    {
        private readonly JwtHelper _jwtHelper;
        private readonly HashingHelper _hashingHelper;
        private readonly EmailSenderHelper _emailSenderHelper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public QuizService(IUnitOfWork unitOfWork, JwtHelper jwtHelper, HashingHelper hashingHelper, EmailSenderHelper emailSenderHelper, IConfiguration configuration)
        {
            _jwtHelper = jwtHelper;
            _hashingHelper = hashingHelper;
            _emailSenderHelper = emailSenderHelper;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<ResponseDTO> CreateNewQuizAsync(CreateQuizDTO quizDTO)
        {
            var quizzes = await _unitOfWork.QuizRepository.GetAll();

            var allChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var resultLink = "";
            bool isLinkUnique = false;

            do
            {
                resultLink = new string(
                   Enumerable.Repeat(allChar, 8)
                   .Select(token => token[random.Next(token.Length)]).ToArray());

                isLinkUnique = true;

                foreach (var quizDetails in quizzes)
                {
                    if (quizDetails.QuizLink.Equals(resultLink, StringComparison.OrdinalIgnoreCase))
                    {
                        isLinkUnique = false;
                        break;
                    }
                }
            } while (!isLinkUnique);

            var token = _jwtHelper.DecodeToken();
            var adminUsername = token.Username;

            var admin = (await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(u => u.Username == adminUsername));

            if (admin != null)
            {
                Quiz quiz = new()
                {
                    Title = quizDTO.Title,
                    Description = quizDTO.Description,
                    CategoryId = quizDTO.CategoryId,
                    ScheduledDate = quizDTO.ScheduleDate,
                    TotalQuestion = quizDTO.TotalQuestion,
                    MarksPerQuestion = quizDTO.MarksPerQuestion,
                    NegativePerQuestion = quizDTO.NegativePerQuestion,
                    TotalMarks = quizDTO.TotalMarks,
                    MinMarks = quizDTO.MinMarks,
                    StatusId = 1,
                    WinningAmount = quizDTO.WinningAmount,
                    IsDeleted = false,
                    CreatedBy = admin.UserId,
                    CreatedDate = DateTime.Now,
                    QuizLink = resultLink,
                    DifficultyId = quizDTO.DifficultyId,
                };

                await _unitOfWork.QuizRepository.CreateAsync(quiz);
                await _unitOfWork.SaveAsync();

                return new()
                {
                    IsSuccess = true,
                    Message = "Quiz Details added successfully!!",
                    Data = quiz.QuizLink,
                    StatusCode = HttpStatusCode.OK
                };
            }

            return new()
            {
                IsSuccess = false,
                Message = "Something went wrong!!",
                StatusCode = HttpStatusCode.BadRequest
            };
        }

        public async Task<ResponseDTO> GetQuizzesFilterAsync(GetQuizFilterDTO getQuizFilterDTO)
        {
            var query = from q in _unitOfWork.QuizRepository.GetTable()
                        where q.IsDeleted == false
                        && (getQuizFilterDTO.SearchValue == string.Empty || q.Title.ToLower().Contains(getQuizFilterDTO.SearchValue.ToLower())
                        || q.Description.ToLower().Contains(getQuizFilterDTO.SearchValue.ToLower()))
                        && (getQuizFilterDTO.StatusId == 0 || q.StatusId == getQuizFilterDTO.StatusId)
                        && (getQuizFilterDTO.DifficultyId == 0 || q.DifficultyId == getQuizFilterDTO.DifficultyId)
                        && (getQuizFilterDTO.CategoryId == 0 || q.CategoryId == getQuizFilterDTO.CategoryId)
                        orderby q.ScheduledDate ascending
                        select new
                        {
                            q.QuizId,
                            q.Title,
                            q.Description,
                            q.CategoryId,
                            q.DifficultyId,
                            q.ScheduledDate,
                            q.QuizLink,
                            q.Category.CategoryName,
                            q.TotalMarks,
                            q.TotalQuestion,
                            q.WinningAmount
                        };

            var quizzes = await query.ToListAsync().ConfigureAwait(false);

            List<GetQuizDTO> getQuizDTOs = new List<GetQuizDTO>();

            foreach (var quiz in quizzes)
            {
                GetQuizDTO getQuizDTO = new()
                {
                    QuizId = quiz.QuizId,
                    Title = quiz.Title,
                    Description = quiz.Description,
                    ScheduledDate = quiz.ScheduledDate,
                    CategoryId = quiz.CategoryId,
                    DifficultyId = quiz.DifficultyId,
                    QuizLink = quiz.QuizLink,
                    CategoryName = quiz.CategoryName,
                    TotalMarks = quiz.TotalMarks,
                    TotalQuestion = quiz.TotalQuestion,
                    WinningAmount = quiz.WinningAmount
                };

                getQuizDTOs.Add(getQuizDTO);
            }

            int recordSize = Int32.Parse(_configuration["Records:Size"]);
            int totalCount = getQuizDTOs.Count;
            int totalPage = (int)Math.Ceiling((double)totalCount / recordSize);

            List<GetQuizDTO> sendQuizDTOs = getQuizDTOs
            .Skip((getQuizFilterDTO.CurrentPage - 1) * recordSize)
            .Take(recordSize)
            .ToList();

            if (totalCount == 0)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "No Quizzes Found!!",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            PaginationDTO paginationDTO = new()
            {
                TotalCount = totalCount,
                TotalPages = totalPage,
                CurrentPage = getQuizFilterDTO.CurrentPage,
                PageSize = 3,
                RecordSize = recordSize
            };

            AllQuizDTO allQuizDTO = new()
            {
                GetQuizzes = sendQuizDTOs,
                Pagination = paginationDTO
            };

            return new()
            {
                IsSuccess = true,
                Message = "Quiz Details fetched successfully!!",
                Data = allQuizDTO,
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> AddQuizQuestionsAsync(QuizQuestionsDTO quizQuestionsDTO)
        {
            Quiz? quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(u => u.QuizLink == quizQuestionsDTO.QuizLink);

            if (quiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "No Quizzes Found!!",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            foreach (var questionDTO in quizQuestionsDTO.QuestionDTOs)
            {
                Question question = new()
                {
                    QuizId = quiz.QuizId,
                    QuestionTypeId = questionDTO.QuestionTypeId,
                    QuestionText = questionDTO.QuestionText,
                    IsDeleted = false
                };

                await _unitOfWork.QuestionRepository.CreateAsync(question);
                await _unitOfWork.SaveAsync();

                if (questionDTO.QuestionTypeId == 1 || questionDTO.QuestionTypeId == 2)
                {
                    int count = 0;
                    foreach (var optionList in questionDTO.Options)
                    {
                        count++;
                        Option option = new()
                        {
                            QuestionId = question.QuestionId,
                            OptionText = optionList.OptionText,
                            IsAnswer = optionList.IsAnswer,
                            OptionNo = count,
                        };

                        await _unitOfWork.OptionRepository.CreateAsync(option);
                        await _unitOfWork.SaveAsync();
                    }
                }
                else if (questionDTO.QuestionTypeId == 3)
                {
                    Option option1 = new()
                    {
                        QuestionId = question.QuestionId,
                        OptionText = "True",
                        IsAnswer = questionDTO.IsTrue,
                        OptionNo = 1,
                    };

                    await _unitOfWork.OptionRepository.CreateAsync(option1);

                    Option option2 = new()
                    {
                        QuestionId = question.QuestionId,
                        OptionText = "False",
                        IsAnswer = !questionDTO.IsTrue,
                        OptionNo = 2,
                    };

                    await _unitOfWork.OptionRepository.CreateAsync(option2);
                    await _unitOfWork.SaveAsync();
                }
            }

            return new()
            {
                IsSuccess = true,
                Message = "Questions Added Successfully!!",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> GetQuizStatusCountAsync()
        {
            int pendingCount = await _unitOfWork.QuizRepository.CountAsync(u => u.StatusId == 1 && u.IsDeleted==false);
            int upcomingCount = await _unitOfWork.QuizRepository.CountAsync(u => u.StatusId == 2 && u.IsDeleted == false);
            int activeCount = await _unitOfWork.QuizRepository.CountAsync(u => u.StatusId == 3 && u.IsDeleted == false);
            int completedCount = await _unitOfWork.QuizRepository.CountAsync(u => u.StatusId == 4 && u.IsDeleted == false);

            StatusCountDTO statusCountDTO = new()
            {
                PendingCount = pendingCount,
                UpcomingCount = upcomingCount,
                ActiveCount = activeCount,
                CompletedCount = completedCount
            };

            return new()
            {
                IsSuccess = true,
                Data = statusCountDTO,
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> GetQuizDifficultiesAsync()
        {
            List<QuizDifficulty> quizDifficulties = await _unitOfWork.QuizDifficultyRepository.GetAll();

            return new()
            {
                IsSuccess = true,
                Data = quizDifficulties,
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> GetQuizCategoriesAsync()
        {
            List<QuizCategory> quizCategories = await _unitOfWork.QuizCategoryRepository.GetAll();

            return new()
            {
                IsSuccess = true,
                Data = quizCategories,
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> GetQuizDetailsAsync(string quizLink)
        {
            var query = (from q in _unitOfWork.QuizRepository.GetTable()
                         where q.IsDeleted == false
                         && q.QuizLink == quizLink
                         select new
                         {
                             q.QuizId,
                             q.Title,
                             q.Description,
                             q.CategoryId,
                             q.DifficultyId,
                             q.ScheduledDate,
                             q.TotalQuestion,
                             q.MarksPerQuestion,
                             q.NegativePerQuestion,
                             q.TotalMarks,
                             q.MinMarks,
                             q.WinningAmount,
                             q.QuizLink,
                             q.Category.CategoryName,
                         });

            var quiz = await query.FirstOrDefaultAsync().ConfigureAwait(false);

            if (quiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "QuizLink is Invalid!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            GetQuizDTO getQuizDTO = new()
            {
                QuizId = quiz.QuizId,
                CategoryId = quiz.CategoryId,
                DifficultyId = quiz.DifficultyId,
                ScheduledDate = quiz.ScheduledDate,
                TotalQuestion = quiz.TotalQuestion,
                MarksPerQuestion = quiz.MarksPerQuestion,
                WinningAmount = quiz.WinningAmount,
                CategoryName = quiz.CategoryName,
                Description = quiz.Description,
                MinMarks = quiz.MinMarks,
                NegativePerQuestion = quiz.NegativePerQuestion,
                QuizLink = quiz.QuizLink,
                Title = quiz.Title,
                TotalMarks = quiz.TotalMarks
            };

            return new()
            {
                IsSuccess = true,
                Message = "Quiz Details Fetched Successfully!!",
                Data = getQuizDTO,
                StatusCode = HttpStatusCode.OK,
            };
        }

        public async Task<ResponseDTO> GetQuizQuestionsAsync(string quizLink)
        {
            var query = (from q in _unitOfWork.QuizRepository.GetTable()
                         join qu in _unitOfWork.QuestionRepository.GetTable() on q.QuizId equals qu.QuizId
                         join a in _unitOfWork.OptionRepository.GetTable() on qu.QuestionId equals a.QuestionId into OptionsGroup
                         where q.IsDeleted == false && qu.IsDeleted == false
                         && q.QuizLink == quizLink
                         select new
                         {
                             qu.QuestionId,
                             q.QuizId,
                             qu.QuestionTypeId,
                             qu.QuestionText,
                             Options = OptionsGroup.OrderBy(o => o.OptionNo).ToList()
                         });

            var quizQuestions = await query.ToListAsync().ConfigureAwait(false);

            if (quizQuestions.Count() == 0)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "No Such Quiz Exists!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            List<GetQuestionsDTO> getQuestionsDTOs = new();

            foreach (var quizQuestion in quizQuestions)
            {
                GetQuestionsDTO getQuestionsDTO = new()
                {
                    QuestionId = quizQuestion.QuestionId,
                    QuizId = quizQuestion.QuizId,
                    QuestionTypeId = quizQuestion.QuestionTypeId,
                    QuestionText = quizQuestion.QuestionText,
                    Options = quizQuestion.Options
                };
                getQuestionsDTOs.Add(getQuestionsDTO);
            }

            if (getQuestionsDTOs == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "No Questions Found!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            return new()
            {
                IsSuccess = true,
                Message = "Quiz Questions Fetched Successfully!!",
                Data = getQuestionsDTOs,
                StatusCode = HttpStatusCode.OK,
            };
        }

        public async Task<ResponseDTO> UpdateQuizDetailsAsync(UpdateQuizDetailsDTO updateQuizDetailsDTO)
        {
            Quiz? quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(u => u.QuizLink == updateQuizDetailsDTO.QuizLink);

            var token = _jwtHelper.DecodeToken();
            var username = token.Username;

            var user = (await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(u => u.Username == username));

            if (quiz == null || user == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "QuizLink is Invalid!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            quiz.Title = updateQuizDetailsDTO.Title;
            quiz.Description = updateQuizDetailsDTO.Description;
            quiz.ScheduledDate = updateQuizDetailsDTO.ScheduleDate;
            quiz.CategoryId = updateQuizDetailsDTO.CategoryId;
            quiz.DifficultyId = updateQuizDetailsDTO.DifficultyId;
            quiz.WinningAmount = updateQuizDetailsDTO.WinningAmount;
            quiz.ModifiedBy = user.UserId;
            quiz.ModifiedDate = DateTime.Now;

            await _unitOfWork.SaveAsync();

            return new()
            {
                IsSuccess = true,
                Message = "Quiz Details updated successfully!!",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> PublishQuizAsync(string quizLink)
        {
            Quiz? quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(u => u.QuizLink == quizLink);

            if (quiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "QuizLink is Invalid!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            quiz.StatusId = 2;
            quiz.ModifiedDate = DateTime.Now;

            await _unitOfWork.SaveAsync();

            return new()
            {
                IsSuccess = true,
                Message = "Quiz Published successfully!!",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> DeleteQuizAsync(string quizLink)
        {
            Quiz? quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(u => u.QuizLink == quizLink);

            var token = _jwtHelper.DecodeToken();
            var username = token.Username;

            var user = (await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(u => u.Username == username));

            if (quiz == null || user == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "QuizLink is Invalid!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            quiz.IsDeleted = true;
            quiz.ModifiedBy = user.UserId;
            quiz.ModifiedDate = DateTime.Now;

            await _unitOfWork.SaveAsync();

            return new()
            {
                IsSuccess = true,
                Message = "Quiz Deleted successfully!!",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> UpdateQuizQuestionAsync(UpdateQuestionDetailsDTO updateQuestionDetailsDTO)
        {
            Question? question = await _unitOfWork.QuestionRepository.GetFirstOrDefaultAsync(u => u.QuestionId == updateQuestionDetailsDTO.QuestionId && u.IsDeleted == false);

            var token = _jwtHelper.DecodeToken();
            var username = token.Username;

            var user = (await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(u => u.Username == username));

            if (question == null || user == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Question not found!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            question.QuestionText = updateQuestionDetailsDTO.QuestionText;

            List<Option> options = await _unitOfWork.OptionRepository.GetWhereAsync(u => u.QuestionId == updateQuestionDetailsDTO.QuestionId);
            options = options.OrderBy(u => u.OptionNo).ToList();

            if (question.QuestionTypeId == 1 || question.QuestionTypeId == 2)
            {
                int count = 0;
                foreach (var option in options)
                {
                    option.OptionText = updateQuestionDetailsDTO.Options.ElementAt(count).OptionText;
                    option.IsAnswer = updateQuestionDetailsDTO.Options.ElementAt(count).IsAnswer;
                    count++;
                }
            }
            else if (question.QuestionTypeId == 3)
            {
                options.ElementAt(0).IsAnswer = updateQuestionDetailsDTO.IsTrue;
                options.ElementAt(1).IsAnswer = !updateQuestionDetailsDTO.IsTrue;
            }

            await _unitOfWork.SaveAsync();

            return new()
            {
                IsSuccess = true,
                Message = "Quiz Question updated successfully!!",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> DeleteQuizQuestionAsync(int questionId)
        {
            Question? question = await _unitOfWork.QuestionRepository.GetFirstOrDefaultAsync(u => u.QuestionId == questionId && u.IsDeleted == false);

            if (question == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Question not found!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            question.IsDeleted = true;
            await _unitOfWork.SaveAsync();

            return new()
            {
                IsSuccess = true,
                Message = "Quiz Question deleted successfully!!",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> GetSingleQuestion(GetSingleQuestionDTO getSingleQuestionDTO)
        {
            Quiz quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(u => u.QuizLink == getSingleQuestionDTO.QuizLink && u.IsDeleted == false && (u.StatusId == 2 || u.StatusId == 3));
            List<Question> questions = await _unitOfWork.QuestionRepository.GetWhereAsync(u => u.QuizId == quiz.QuizId);
            Question? singleQuestion = questions.Skip(getSingleQuestionDTO.QuestionCount).Take(1).FirstOrDefault();
            List<Option> options = await _unitOfWork.OptionRepository.GetWhereAsync(u => u.QuestionId == singleQuestion.QuestionId);
            if (singleQuestion == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Question not found!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            GetQuestionsDTO getQuestionsDTO = new()
            {
                QuestionId = singleQuestion.QuestionId,
                QuestionText = singleQuestion.QuestionText,
                QuestionTypeId = singleQuestion.QuestionTypeId,
                QuizId = quiz.QuizId,
                Options = options
            };
            return new()
            {
                IsSuccess = true,
                Message = "Quiz Details fetched successfully!!",
                Data = getQuestionsDTO,
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseDTO> GetCountOfQuestions(string quizLink)
        {
            Quiz? quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(u => u.QuizLink == quizLink);
            if (quiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Quiz not found!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }
            List<Question> questions = await _unitOfWork.QuestionRepository.GetWhereAsync(u => u.QuizId == quiz.QuizId);
            if (questions.Count() <= 0) {
                return new()
                {
                    IsSuccess = false,
                    Message = "Questions not found!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }
            return new()
            {
                IsSuccess = true,
                Message = "Questions not found!!",
                Data = questions.Count(),
                StatusCode = HttpStatusCode.BadRequest,
            };
        }

        public async Task<ResponseDTO> GetQuizTime(string QuizLink)
        {
            var Data=await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(r=>r.QuizLink==QuizLink);
            if (Data == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Questions not found!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }
              return new()
            {
                IsSuccess = true,
                Data = Data.ScheduledDate,
                StatusCode = HttpStatusCode.OK,
            };
        }

        public  List<KeyValuePair<int, string>> GetActiveQuizzes()
        {
            var QuizTable = _unitOfWork.QuizRepository.GetTable();
            List<KeyValuePair<int,string>> activeQuizzes = new List<KeyValuePair<int, string>>();
            foreach(var Quiz in QuizTable)
            {
                if(Quiz.StatusId == 3)
                {
                    activeQuizzes.Add(new KeyValuePair<int, string>(3,Quiz.QuizLink!));
                }
                else if(Quiz.StatusId == 4) {
                    activeQuizzes.Add(new KeyValuePair<int, string>(4, Quiz.QuizLink!));
                }

            }

            return activeQuizzes;
        }
        public async Task<ResponseDTO> GetAllQuestions(string QuizLink)
        {
            if (QuizLink == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Questions not found!!",
                    StatusCode = HttpStatusCode.BadRequest,
                    Data = null,
                };
            }
            Quiz? CurrentQuiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(r => r.QuizLink == QuizLink);
            if(CurrentQuiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Questions not found!!",
                    StatusCode = HttpStatusCode.BadRequest,
                    Data=null
                };
            }
            List<Question> Questions =await _unitOfWork.QuestionRepository.GetWhereAsync(r => r.QuizId ==CurrentQuiz!.QuizId);
            List<GetQuestionsDTO> getAllQuestions=new List<GetQuestionsDTO>();
             
            foreach (var Question in Questions)
            {
                List<Option> Options = await _unitOfWork.OptionRepository.GetWhereAsync(r => r.QuestionId == Question.QuestionId);
                GetQuestionsDTO getQuestionsDTO = new()
                {
                    Question= Question,
                    QuestionTypeId=Question.QuestionTypeId,
                    QuestionText= Question.QuestionText,
                    QuizId=Question.QuizId,
                    QuestionId=Question.QuestionId,
                    Options=Options
                };
                getAllQuestions.Add(getQuestionsDTO);
            }
            return new()
            {
                IsSuccess = true,
                Data = getAllQuestions,
                StatusCode = HttpStatusCode.OK,
            };
        }

        public async Task<ResponseDTO> GetCorrectAnswer(int id)
        {
            if(id == 0)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Questions not found!!",
                    StatusCode = HttpStatusCode.BadRequest,
                    Data = null
                };
            }
            Option option = await _unitOfWork.OptionRepository.GetFirstOrDefaultAsync(o => o.QuestionId == id && o.IsAnswer == true);
            return new()
            {
                IsSuccess = true,
                Message = "Correct option found",
                StatusCode = HttpStatusCode.OK,
                Data = option
            };
        }
    }
}
