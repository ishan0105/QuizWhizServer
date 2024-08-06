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
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace QuizWhiz.Application.Services
{
    public class QuizService : IQuizService
    {
        private readonly JwtHelper _jwtHelper;
        private readonly HashingHelper _hashingHelper;
        private readonly EmailSenderHelper _emailSenderHelper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<QuizHub> _hubContext;
        private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new ConcurrentDictionary<string, string>();
        public QuizService(IUnitOfWork unitOfWork, JwtHelper jwtHelper, HashingHelper hashingHelper, EmailSenderHelper emailSenderHelper, IConfiguration configuration,IHubContext<QuizHub>hubContext)
        {
            _jwtHelper = jwtHelper;
            _hashingHelper = hashingHelper;
            _emailSenderHelper = emailSenderHelper;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _hubContext = hubContext;
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
                    if (quizDetails.QuizLink != null && quizDetails.QuizLink.Equals(resultLink, StringComparison.OrdinalIgnoreCase))
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

            if(!string.IsNullOrEmpty(getQuizFilterDTO.FilterBy))
            {
                switch(getQuizFilterDTO.FilterBy.ToLower())
                {
                    case "title":
                        query = getQuizFilterDTO.IsAscending ? query.OrderBy(q => q.Title) : query.OrderByDescending(q => q.Title);
                        break;
                    case "totalquestions":
                        query = getQuizFilterDTO.IsAscending ? query.OrderBy(q => q.TotalQuestion) : query.OrderByDescending(q => q.TotalQuestion);
                        break;
                    case "totalmarks":
                        query = getQuizFilterDTO.IsAscending ? query.OrderBy(q => q.TotalMarks) : query.OrderByDescending(q => q.TotalMarks);
                        break;
                    case "difficulty":
                        query = getQuizFilterDTO.IsAscending ? query.OrderBy(q => q.DifficultyId) : query.OrderByDescending(q => q.DifficultyId);
                        break;
                    default:
                        query = query.OrderBy(q => q.ScheduledDate); 
                        break;
                }
            }
            else
            {
                query = getQuizFilterDTO.IsAscending ? query.OrderBy(q => q.ScheduledDate) : query.OrderByDescending(q => q.ScheduledDate);
            }

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

            int recordSize = Int32.Parse(_configuration["Records:Size"]!);
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
            int pendingCount = await _unitOfWork.QuizRepository.CountAsync(u => u.StatusId == 1 && u.IsDeleted == false);
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
            var questions = await _unitOfWork.QuestionRepository.GetWhereAsync(r => r.QuizId == quiz.QuizId && r.IsDeleted == false);
            int questionsCount = questions.Count;
            if (quiz.TotalQuestion != questionsCount)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Add Remaining Questions!!",
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
            if (user == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Not Access!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }
            if (updateQuestionDetailsDTO.QuestionId == 0 && question == null)
            {
                Question newQuestion = new()
                {
                    QuizId = updateQuestionDetailsDTO.QuizId,
                    QuestionTypeId = updateQuestionDetailsDTO.QuestionTypeId,
                    QuestionText = updateQuestionDetailsDTO.QuestionText,
                    IsDeleted = false
                };

                await _unitOfWork.QuestionRepository.CreateAsync(newQuestion);
                await _unitOfWork.SaveAsync();

                if (updateQuestionDetailsDTO.QuestionTypeId == 1 || updateQuestionDetailsDTO.QuestionTypeId == 2)
                {
                    int count = 0;
                    foreach (var optionList in updateQuestionDetailsDTO.Options)
                    {
                        count++;
                        Option option = new()
                        {
                            QuestionId = newQuestion.QuestionId,
                            OptionText = optionList.OptionText,
                            IsAnswer = optionList.IsAnswer,
                            OptionNo = count,
                        };

                        await _unitOfWork.OptionRepository.CreateAsync(option);
                        await _unitOfWork.SaveAsync();
                    }
                }
                else if (updateQuestionDetailsDTO.QuestionTypeId == 3)
                {
                    Option option1 = new()
                    {
                        QuestionId = newQuestion.QuestionId,
                        OptionText = "True",
                        IsAnswer = updateQuestionDetailsDTO.IsTrue,
                        OptionNo = 1,
                    };

                    await _unitOfWork.OptionRepository.CreateAsync(option1);

                    Option option2 = new()
                    {
                        QuestionId = newQuestion.QuestionId,
                        OptionText = "False",
                        IsAnswer = !updateQuestionDetailsDTO.IsTrue,
                        OptionNo = 2,
                    };

                    await _unitOfWork.OptionRepository.CreateAsync(option2);
                    await _unitOfWork.SaveAsync();
                }

            }
            else
            {

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

              
            }
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
            Quiz? quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(u => u.QuizLink == getSingleQuestionDTO.QuizLink && u.IsDeleted == false && (u.StatusId == 2 || u.StatusId == 3));
            List<Question> questions = await _unitOfWork.QuestionRepository.GetWhereAsync(u => u.QuizId == quiz!.QuizId);
            Question? singleQuestion = questions.Skip(getSingleQuestionDTO.QuestionCount).Take(1).FirstOrDefault();
            List<Option> options = await _unitOfWork.OptionRepository.GetWhereAsync(u => u.QuestionId == singleQuestion!.QuestionId);
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
                QuizId = quiz!.QuizId,
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
            if (questions.Count() <= 0)
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
                Message = "Questions not found!!",
                Data = questions.Count(),
                StatusCode = HttpStatusCode.BadRequest,
            };
        }

        public async Task<ResponseDTO> GetQuizTime(string QuizLink)
        {
            var Data = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(r => r.QuizLink == QuizLink);
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

        //public async Task<ResponseDTO> GetCorrectAnswer(string quizLink, int questionCount)

        public List<KeyValuePair<int, string>> GetActiveQuizzes()
        {
            var QuizTable = _unitOfWork.QuizRepository.GetTable();
            List<KeyValuePair<int, string>> activeQuizzes = new List<KeyValuePair<int, string>>();
            foreach (var Quiz in QuizTable)
            {
                if (Quiz.StatusId == 3)
                {
                    activeQuizzes.Add(new KeyValuePair<int, string>(3, Quiz.QuizLink!));
                }
                else if (Quiz.StatusId == 4)
                {
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
                };
            }
            Quiz? CurrentQuiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(r => r.QuizLink == QuizLink);
            if (CurrentQuiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Questions not found!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }
            List<Question> Questions = await _unitOfWork.QuestionRepository.GetWhereAsync(r => r.QuizId == CurrentQuiz!.QuizId && r.IsDeleted == false);
            List<GetQuestionsDTO> getAllQuestions = new List<GetQuestionsDTO>();

            foreach (var Question in Questions)
            {
                List<Option> Options = await _unitOfWork.OptionRepository.GetWhereAsync(r => r.QuestionId == Question.QuestionId);
                GetQuestionsDTO getQuestionsDTO = new()
                {
                    Question = Question,
                    QuestionTypeId = Question.QuestionTypeId,
                    QuestionText = Question.QuestionText,
                    QuizId = Question.QuizId,
                    QuestionId = Question.QuestionId,
                    Options = Options
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
            if (id == 0)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Questions not found!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }
            List<Option> option = await _unitOfWork.OptionRepository.GetWhereAsync(o => o.QuestionId == id && o.IsAnswer == true);
            return new()
            {
                IsSuccess = true,
                Message = "Correct option found",
                StatusCode = HttpStatusCode.OK,
                Data = option
            };
        }

        public async Task<ResponseDTO> UpdateScore(string QuizLink, string userName, int QuestionId, List<int> userAnswers)
        {
            if (QuestionId == 0)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Questions not found!!",
                    StatusCode = HttpStatusCode.OK,
                };
            }
            List<Option> Options = await _unitOfWork.OptionRepository.GetWhereAsync(o => o.QuestionId == QuestionId && o.IsAnswer == true);
            List<int> OptionIds = Options.Select(o => o.OptionNo).ToList();
            userAnswers.Sort();
            OptionIds.Sort();
            bool IsCorrect = true;

            if (userAnswers.Count != OptionIds.Count)
            {
                IsCorrect = false;
            }
            else
            {
                var idx = 0;
                foreach (var option in OptionIds)
                {
                    if (option != userAnswers.ElementAt(idx))
                    {
                        IsCorrect = false;
                        break;
                    }
                    ++idx;
                }
            }
            Quiz? quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(q => q.QuizLink == QuizLink);
            if (quiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Quiz not found",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }
            User? user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(u => u.Username == userName);
            if (user == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User not found",
                    StatusCode = HttpStatusCode.OK,
                };
            }

            QuizParticipants? quizParticipants = await _unitOfWork.QuizParticipantsRepository.GetFirstOrDefaultAsync(qp => qp.QuizId == quiz.QuizId && qp.UserId == user.UserId);

            if (quizParticipants == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Something Went Wrong",
                    StatusCode = HttpStatusCode.OK,
                };
            }
            if (quizParticipants.IsDisqualified)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User has been disqualified",
                    StatusCode = HttpStatusCode.OK,
                };
            }
            if (IsCorrect)
            {
                quizParticipants.CorrectQuestions = quizParticipants.CorrectQuestions + 1;
                quizParticipants.TotalScore = quizParticipants.TotalScore + 1;
            }
            else
            {
                quizParticipants.IsDisqualified = true;
            }
            await _unitOfWork.SaveAsync();

            return new()
            {
                IsSuccess = true,
                Message = "Correct Ans",
                StatusCode = HttpStatusCode.OK,
                Data = IsCorrect
            };
        }
        public async Task<ResponseDTO> GetQuiz(string quizLink)
        {
            if (quizLink == null || quizLink == "")
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Quiz not found!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            Quiz? quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(q => q.QuizLink == quizLink);
            if(quiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Quiz not found!!",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            return new()
            {
                IsSuccess = true,
                Message = "Quiz found!!",
                Data = quiz,
                StatusCode = HttpStatusCode.OK
            };
        }
        public async Task<ResponseDTO> UpdateLeaderBoard(int QuizId)
        {
            if (QuizId == 0)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Quiz not found!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            List<QuizParticipants> quizParticipants=await _unitOfWork.QuizParticipantsRepository.GetWhereAsync(r=>r.QuizId== QuizId);
            var sortedParticipants= quizParticipants.OrderByDescending(e => e.TotalScore).ToList();

            int Rank = 1,TotalFirst=0,TotalSecond=0,TotalThird=0;
           
            for (var idx=0;idx<sortedParticipants.Count; idx++)
            {
                var currentParticipant = sortedParticipants.ElementAt(idx);
                while(idx< sortedParticipants.Count &&  currentParticipant.TotalScore== sortedParticipants.ElementAt(idx).TotalScore)
                {
                    var user = sortedParticipants.ElementAt(idx);
                    var participant = quizParticipants.Find(r=>r.UserId== user.UserId);
                    if (participant == null)
                    {
                        return new()
                        {
                            IsSuccess = false,
                            Message = "user not found!!",
                            StatusCode = HttpStatusCode.BadRequest,
                        };
                    }
                    participant.Rank = Rank;
                    if (Rank == 1) TotalFirst++;
                    else if (Rank == 2) TotalSecond++;
                    else if (Rank == 3) TotalThird++;
                    idx++;
                }
                Rank++;
                idx--;
            }

            var TotalTopThreeRankers = TotalFirst + TotalSecond + TotalThird;
            if (TotalTopThreeRankers != 0)
            {
                var FirstRankProportion = (TotalFirst*1.0 / TotalTopThreeRankers) ;
                var SecondRankProportion = (TotalSecond * 1.0 / TotalTopThreeRankers) * (1 / 2.0);
                var ThirdRankProportion = (TotalThird * 1.0 / TotalTopThreeRankers) * (1 / 3.0);

                /* var a = (TotalFirst / TotalTopThreeRankers) * 1.0;
                 var b = (TotalSecond / TotalTopThreeRankers) * 1.0 * (1 / 2.0);
                 var c = (TotalThird / TotalTopThreeRankers) * 1.0 * (1 / 3.0);*/

                var TotalProportion = FirstRankProportion + SecondRankProportion + ThirdRankProportion;

                var NormalizedFirstProportion = FirstRankProportion / TotalProportion;
                var NormalizedSecondProportion =  SecondRankProportion / TotalProportion;
                var NormalizedThirdProportion = ThirdRankProportion / TotalProportion;

                var quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(q => q.QuizId == QuizId);
                var WinningPrice = 0;

                if (quiz != null)
                {
                    WinningPrice = (int)quiz.WinningAmount;
                }
                var FirstRankPrice = NormalizedFirstProportion * WinningPrice; // 800
                var SecondRankPrice = NormalizedSecondProportion * WinningPrice; //500
                var ThirdRankPrice = NormalizedThirdProportion * WinningPrice; // 600
                for (var idx = 0; idx < sortedParticipants.Count; idx++)
                {
                    var currentParticipant = sortedParticipants.ElementAt(idx);
                    if (currentParticipant.Rank == 1)
                    {
                        currentParticipant.WinningAmount = (int)(FirstRankPrice / TotalFirst);
                    }
                    else if (currentParticipant.Rank == 2)
                    {
                        currentParticipant.WinningAmount = (int)(SecondRankPrice / TotalSecond);
                    }
                    else if (currentParticipant.Rank == 3)
                    {
                        currentParticipant.WinningAmount = (int)(ThirdRankPrice / TotalThird);
                    }
                    else break;
                }
            }
            await _unitOfWork.SaveAsync();

            return new()
            {
                IsSuccess = true,
                Message = "Leaderboard Updated!!",
                StatusCode = HttpStatusCode.OK,
            };
        }
        public async Task<ResponseDTO> GetQuizWinners(string quizLink)
        {
            List<QuizParticipants> firstRank = new List<QuizParticipants>();
            List<QuizParticipants> secondRank = new List<QuizParticipants>();
            List<QuizParticipants> thirdRank = new List<QuizParticipants>();
            Quiz? quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(q => q.QuizLink == quizLink);
            if (quiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Quiz not found",
                    Data = null,
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            if (quiz.StatusId != 4)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Quiz not completed",
                    Data = null,
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            var distinctScores = await _unitOfWork.QuizParticipantsRepository.GetTable().Where(p => p.QuizId == quiz.QuizId).Select(p => p.TotalScore).Distinct().OrderByDescending(score => score).ToListAsync();
            firstRank = await _unitOfWork.QuizParticipantsRepository.GetWhereAsync(q => q.TotalScore == distinctScores.Skip(0).FirstOrDefault() && q.QuizId == quiz.QuizId);
            secondRank = await _unitOfWork.QuizParticipantsRepository.GetWhereAsync(q => q.TotalScore == distinctScores.Skip(1).FirstOrDefault() && q.QuizId == quiz.QuizId);
            thirdRank = await _unitOfWork.QuizParticipantsRepository.GetWhereAsync(q => q.TotalScore == distinctScores.Skip(2).FirstOrDefault() && q.QuizId == quiz.QuizId);

            QuizWinners quizWinners = new QuizWinners
            {
                firstRankedUsers = firstRank,
                secondRankedUsers = secondRank,
                thirdRankedUsers = thirdRank,
            };

            return new()
            {
                IsSuccess = true,
                Data = quizWinners,
                Message = "Quiz winners fetched successfully",
                StatusCode = HttpStatusCode.OK,
            };
        }
        public async Task<ResponseDTO> RegisterUser(string quizLink, string userName)
        {
            User? user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(u => u.Username == userName);
            if (user == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User Not Found",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            Quiz? quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(q => q.QuizLink == quizLink);
            if (quiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Quiz Not Found",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            QuizParticipants? quizParticipants = await _unitOfWork.QuizParticipantsRepository.GetFirstOrDefaultAsync(qp => qp.QuizId == quiz.QuizId && qp.UserId == user.UserId);
            if (quizParticipants != null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User Has Been Already Registered For This Quiz",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            UserLifeline? userLifelineSkip = await _unitOfWork.UserLifelineRepository.GetFirstOrDefaultAsync(ul => ul.UserId == user.UserId && ul.LifelineId == 1);
            UserLifeline? userLifelineFifty = await _unitOfWork.UserLifelineRepository.GetFirstOrDefaultAsync(ul => ul.UserId == user.UserId && ul.LifelineId == 2);
            UserLifeline? userLifelineHeart = await _unitOfWork.UserLifelineRepository.GetFirstOrDefaultAsync(ul => ul.UserId == user.UserId && ul.LifelineId == 3);

            var isSkipUsed = userLifelineSkip != null && userLifelineSkip.LifelineCount != 0 ? false : true;
            var isFiftyUsed = userLifelineFifty != null && userLifelineFifty.LifelineCount != 0 ? false : true;
            var isHeartUsed = userLifelineHeart != null && userLifelineHeart.LifelineCount != 0 ? false : true;

            QuizParticipants newUser = new()
            {
                Quiz = quiz,
                QuizId = quiz.QuizId,
                UserId = user.UserId,
                User = user,
                IsSkipUsed = isSkipUsed,
                IsHeartUsed = isHeartUsed,
                IsFiftyUsed = isFiftyUsed,
            };
            await _unitOfWork.QuizParticipantsRepository.CreateAsync(newUser);
            await _unitOfWork.SaveAsync();

            return new()
            {
                IsSuccess = true,
                Message = "User Registered For Quiz Successfully",
                StatusCode = HttpStatusCode.BadRequest,
                Data = newUser
            };
        }
        public async Task<ResponseDTO> GetDisqualifiedUsers(string quizLink)
        {
            if(quizLink == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Quiz Not Found",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }
            Quiz? quiz=await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(r=>r.QuizLink==quizLink);
            if (quiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Quiz Not Found",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }
            List<QuizParticipants> quizParticipants = await _unitOfWork.QuizParticipantsRepository.GetWhereAsync(r => r.QuizId == quiz.QuizId && r.IsDisqualified==true);
            List<string> disqualifiedUsers = new List<string>();
            foreach(var user in  quizParticipants)
            {
                var currentUser =await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(r => r.UserId == user.UserId);
                if(currentUser == null)
                {
                    return new()
                    {
                        IsSuccess = false,
                        Message = "User Not Found",
                        StatusCode = HttpStatusCode.BadRequest,
                    };
                }
                disqualifiedUsers.Add(currentUser.Username);
            }
            return new()
            {
                IsSuccess = true,
                Message = "Disqualified Users",
                StatusCode = HttpStatusCode.OK,
                Data = disqualifiedUsers
            };
        }
        public async Task<ResponseDTO> GetCoinsAndLifeLineCount(string userName)
        {
            if (userName == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User Not Found",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }
            User? user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(r=>r.Username==userName);

            if (user == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User Not Found",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }
            var coins = await _unitOfWork.UserCoinsRepository.GetFirstOrDefaultAsync(r => r.UserId == user.UserId);
            if(coins == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User Don't Have Coins",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }
            var UserLifelines = await _unitOfWork.UserLifelineRepository.GetWhereAsync(r => r.UserId == user.UserId);
            if(UserLifelines == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User Don't Have Lifelines",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }
            var UserLifelinesCounts = UserLifelines.Select(e => new UserLifeLineDTO
            {
                LifelineId=e.LifelineId,
                LifelineCount=e.LifelineCount,
            }).ToList();

            var LifeLines = await _unitOfWork.LifelineRepository.GetAll();
            CoinsAndLifelineCountDTO coinsAndLifelineCountDTO = new CoinsAndLifelineCountDTO()
            {
                CoinsCount=coins.NoOfCoins,
                UserLifelines= UserLifelinesCounts,
                Lifelines= LifeLines
            };
            return new() {
                IsSuccess = true,
                Message = "Data Fetched Successfully",
                StatusCode = HttpStatusCode.OK,
                Data = coinsAndLifelineCountDTO
            };
        }

        public async Task SendMessageToUser(string userId, string message)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveMessage", message);
        }

        public async Task SendMessageToAllUsers(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
        }

        public IReadOnlyDictionary<string, string> GetConnectedUsers()
        {
            return new ReadOnlyDictionary<string, string>(ConnectedUsers);
        }

        public void AddUser(string connectionId, string userId)
        {
            ConnectedUsers.TryAdd(connectionId, userId);
        }

        public void RemoveUser(string connectionId)
        {
            ConnectedUsers.TryRemove(connectionId, out _);
        }

        public async Task<ResponseDTO> GetUserScoreboard(string quizLink, string username)
        {
            if (username == null || quizLink == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User Not Found",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            User? user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(r => r.Username == username);

            if (user == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User Not Found",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            Quiz? quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(u => u.QuizLink == quizLink);

            if (quiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Quiz Not Found",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            QuizParticipants? quizParticipent = await _unitOfWork.QuizParticipantsRepository.GetFirstOrDefaultAsync(u => u.UserId == user.UserId && u.QuizId == quiz.QuizId);

            if (quizParticipent == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User Did not participate!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            UserScoreboardDTO userScoreboardDTO = new UserScoreboardDTO()
            {
                Username = username,
                Score = quizParticipent.TotalScore,
                TotalScore = quiz.TotalQuestion,
                WinningAmount = quizParticipent.WinningAmount,
                Rank = quizParticipent.Rank,
            };

            return new()
            {
                IsSuccess = true,
                Message = "Data Fetched Successfully",
                StatusCode = HttpStatusCode.OK,
                Data = userScoreboardDTO
            };
        }

        public async Task<ResponseDTO> GetAdminLeaderboard(GetAdminLeaderboardDTO getAdminLeaderboardDTO)
        {
            Quiz? quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(q => q.QuizLink == getAdminLeaderboardDTO.QuizLink);

            if (quiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Quiz not found",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            List<QuizParticipants> listOfparticipant = await _unitOfWork.QuizParticipantsRepository.GetWhereAsync(r => r.QuizId == quiz.QuizId &&
            (getAdminLeaderboardDTO.SearchedWord == "" || r.User.Username.Trim().ToLower().Contains(getAdminLeaderboardDTO.SearchedWord)));

            List<ParticipantsDetailsDTO> quizParticipants = listOfparticipant.Select(qp => new ParticipantsDetailsDTO
            {
                username = _unitOfWork.UserRepository.GetFirstOrDefaultAsync(r => r.UserId == qp.UserId).Result.Username,
                rank = qp.Rank,
                priceWon = qp.WinningAmount,
                score = qp.TotalScore
            }).OrderBy(p => p.rank).ToList();


            int recordSize = Int32.Parse(_configuration["LeaderboardRecords:Size"]!); ;
            int totalCount = quizParticipants.Count;
            int totalPage = (int)Math.Ceiling((double)totalCount / recordSize);

            PaginationDTO paginationDTO = new()
            {
                TotalCount = totalCount,
                TotalPages = totalPage,
                CurrentPage = getAdminLeaderboardDTO.CurrentPage,
                PageSize = getAdminLeaderboardDTO.PageSize,
                RecordSize = recordSize
            };

            List<ParticipantsDetailsDTO> sendQuizLeaderboardDTO = quizParticipants
            .Skip((getAdminLeaderboardDTO.CurrentPage - 1) * recordSize)
            .Take(recordSize)
            .ToList();

            AdminLeaderboardResponseDTO response = new AdminLeaderboardResponseDTO()
            {
                Pagination = paginationDTO,
                QuizParticipants = sendQuizLeaderboardDTO
            };

            return new()
            {
                IsSuccess = true,
                Message = "Quiz Leaderboard fetched successfully",
                StatusCode = HttpStatusCode.OK,
                Data = response
            };
        }

        public async Task<ResponseDTO> GetQuizParticipantsCount(string quizLink)
        {

            if (quizLink == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Quiz Not Found",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }
            Quiz? quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(r => r.QuizLink == quizLink);
            if (quiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Quiz Not Found",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }
            List<QuizParticipants> quizParticipants = await _unitOfWork.QuizParticipantsRepository.GetWhereAsync(r => r.QuizId == quiz.QuizId);

            var countOfParticipants = quizParticipants.Count();

            return new()
            {
                IsSuccess = true,
                Message = "Quiz Participants Fetched Successfully!",
                StatusCode = HttpStatusCode.OK,
                Data = countOfParticipants
            };
        }

        public async Task<ResponseDTO> GetUserLeaderboard(GetUserLeaderboardDTO getUserLeaderboardDTO)
        {
            Quiz? quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(q => q.QuizLink == getUserLeaderboardDTO.QuizLink);

            if (quiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Quiz not found",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }
            List<QuizParticipants> quizParticipants = await _unitOfWork.QuizParticipantsRepository.GetWhereAsync(r => r.QuizId == quiz.QuizId
            && (getUserLeaderboardDTO.SearchedWord == null || r.User.Username.Contains(getUserLeaderboardDTO.SearchedWord.Trim().ToLower()))
            );

            var participatedUsers = await _unitOfWork.QuizParticipantsRepository.GetWhereAsync(r => r.QuizId == quiz.QuizId);
            var totalParticipants = participatedUsers.Count();
            var userPersonalRank = 0;
            var userPersonalScore = 0;


            if (getUserLeaderboardDTO.Username != null)
            {
                User? user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(r => r.Username == getUserLeaderboardDTO.Username);
                if (user != null)
                {
                    var quizParticipant = quizParticipants.FirstOrDefault(r => r.UserId == user!.UserId);
                    if (quizParticipant != null)
                    {
                        userPersonalRank = quizParticipant.Rank;
                        userPersonalScore = quizParticipant.TotalScore;
                    }
                }
            }
            List<QuizParticipantsDTO> quizParticipantsDTO = quizParticipants.OrderBy(r => r.Rank).Select(r => new QuizParticipantsDTO()
            {
                QuizTitle = r.Quiz.Title,
                Username = _unitOfWork.UserRepository.GetFirstOrDefaultAsync(p => p.UserId == r.UserId)!.Result!.Username,
                Rank = r.Rank,
                Score = r.TotalScore,
                Prize = r.WinningAmount,
                TotalParticipants = totalParticipants,
                TotalMarks = r.Quiz.TotalMarks,
                PersonalRank = userPersonalRank,
                PrizeMoney = quiz.WinningAmount,
                AchievedScore = userPersonalScore,
            }).ToList();

            int recordSize = Int32.Parse(_configuration["LeaderboardRecords:Size"]!);
            int totalCount = quizParticipants.Count;
            int totalPage = (int)Math.Ceiling((double)totalCount / recordSize);

            PaginationDTO paginationDTO = new()
            {
                TotalCount = totalCount,
                TotalPages = totalPage,
                CurrentPage = getUserLeaderboardDTO.CurrentPage,
                PageSize = 3,
                RecordSize = recordSize
            };

            List<QuizParticipantsDTO> sendQuizLeaderboardDTO = quizParticipantsDTO
            .Skip((getUserLeaderboardDTO.CurrentPage - 1) * recordSize)
            .Take(recordSize)
            .ToList();

            UserLeaderboardResponseDTO response = new UserLeaderboardResponseDTO()
            {
                Pagination = paginationDTO,
                QuizParticipants = sendQuizLeaderboardDTO
            };

            return new()
            {
                IsSuccess = true,
                Message = "Quiz Leaderboard fetched successfully",
                StatusCode = HttpStatusCode.OK,
                Data = response
            };
        }

        public async Task<ResponseDTO> UnableHeartLifeline(string quizLink, string username)
        {
            if (username == null || quizLink == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User Not Found",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            User? user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(r => r.Username == username);

            if (user == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User Not Found",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            Quiz? quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(u => u.QuizLink == quizLink);

            if (quiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Quiz Not Found",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            QuizParticipants? quizParticipent = await _unitOfWork.QuizParticipantsRepository.GetFirstOrDefaultAsync(u => u.UserId == user.UserId && u.QuizId == quiz.QuizId);

            if (quizParticipent == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "User Did Not Participate!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            if (quizParticipent.IsHeartUsed == true)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "Lifeline Used Already!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            quizParticipent.IsDisqualified = false;
            quizParticipent.IsHeartUsed = true;

            UserLifeline? userLifelineHeart = await _unitOfWork.UserLifelineRepository.GetFirstOrDefaultAsync(ul => ul.UserId == user.UserId && ul.LifelineId == 3);
            if (userLifelineHeart != null)
            {
                userLifelineHeart.LifelineCount--;
            }

            await _unitOfWork.SaveAsync();

            return new()
            {
                IsSuccess = true,
                Message = "Data Updated Successfully",
                StatusCode = HttpStatusCode.OK,

            };
        }
    }
}