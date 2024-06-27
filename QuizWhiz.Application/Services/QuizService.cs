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
                QuizSchedule quizSchedule = new QuizSchedule()
                {
                    ScheduledDate = quizDTO.ScheduleDate,
                    CreatedDate = DateTime.Now,
                };

                await _unitOfWork.QuizScheduleRepository.CreateAsync(quizSchedule);
                await _unitOfWork.SaveAsync();

                Quiz quiz = new()
                {
                    Title = quizDTO.Title,
                    Description = quizDTO.Description,
                    CategoryId = quizDTO.CategoryId,
                    ScheduleId = quizSchedule.ScheduleId,
                    TotalQuestion = quizDTO.TotalQuestion,
                    MarksPerQuestion = quizDTO.MarksPerQuestion,
                    NegativePerQuestion = quizDTO.NegativePerQuestion,
                    TotalMarks = quizDTO.TotalMarks,
                    MinMarks = quizDTO.MinMarks,
                    StatusId = 1,
                    WinningAmount = quizDTO.WinningAmount,
                    IsDeleted = false,
                    CreatedBy = admin.UserId,
                    CreatedDate = quizSchedule.CreatedDate,
                    QuizLink = resultLink,
                    IsPublished = false,
                    DifficultyId = quizDTO.DifficultyId,
                };

                await _unitOfWork.QuizRepository.CreateAsync(quiz);
                await _unitOfWork.SaveAsync();

                return new()
                {
                    IsSuccess = true,
                    Message = "Quiz Details added successfully!!",
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
                        join s in _unitOfWork.QuizScheduleRepository.GetTable()
                        on q.ScheduleId equals s.ScheduleId
                        where q.IsDeleted == false
                        && (getQuizFilterDTO.SearchValue == string.Empty || q.Title.ToLower().Contains(getQuizFilterDTO.SearchValue.ToLower())
                        || q.Description.ToLower().Contains(getQuizFilterDTO.SearchValue.ToLower()))
                        && (getQuizFilterDTO.StatusId == 0 || q.StatusId == getQuizFilterDTO.StatusId)
                        && (getQuizFilterDTO.DifficultyId == 0 || q.DifficultyId == getQuizFilterDTO.DifficultyId)
                        && (getQuizFilterDTO.CategoryId == 0 || q.CategoryId == getQuizFilterDTO.CategoryId)
                        orderby s.ScheduledDate ascending
                        select new
                        {
                            q.QuizId,
                            q.Title,
                            q.Description,
                            q.CategoryId,
                            q.DifficultyId,
                            s.ScheduledDate,
                            q.QuizLink,
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
                    QuizLink = quiz.QuizLink
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

            if(totalCount == 0)
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
            foreach (var questionDTO in quizQuestionsDTO.QuestionDTOs)
            {
                Question question = new()
                {
                    QuizId = quizQuestionsDTO.QuizId,
                    QuestionTypeId = questionDTO.QuestionTypeId,
                    QuestionText = questionDTO.QuestionText,
                    IsDeleted = false
                };

                if (questionDTO.QuestionTypeId == 3 || questionDTO.QuestionTypeId == 4)
                {
                    question.OptionA = questionDTO.OptionA;
                    question.OptionB = questionDTO.OptionB;
                    question.OptionC = questionDTO.OptionC;
                    question.OptionD = questionDTO.OptionD;
                }

                await _unitOfWork.QuestionRepository.CreateAsync(question);
                await _unitOfWork.SaveAsync();

                foreach (var answerText in questionDTO.Answers)
                {
                    Answer answer = new()
                    {
                        QuestionId = question.QuestionId,
                        AnswerText = answerText
                    };

                    await _unitOfWork.AnswerRepository.CreateAsync(answer);
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
            int pendingCount = await _unitOfWork.QuizRepository.CountAsync(u => u.StatusId == 1);
            int upcomingCount = await _unitOfWork.QuizRepository.CountAsync(u => u.StatusId == 2);
            int activeCount = await _unitOfWork.QuizRepository.CountAsync(u => u.StatusId == 3);
            int completedCount = await _unitOfWork.QuizRepository.CountAsync(u => u.StatusId == 4);

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
            Quiz quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(u => u.QuizLink == quizLink);

            if(quiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "QuizLink is Invalid!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            return new()
            {
                IsSuccess = true,
                Message = "Quiz Details Fetched Successfully!!",
                Data = quiz,
                StatusCode = HttpStatusCode.OK,
            };
        }

        public async Task<ResponseDTO> GetQuizCommentsAsync(string quizLink)
        {
            Quiz quiz = await _unitOfWork.QuizRepository.GetFirstOrDefaultAsync(u => u.QuizLink == quizLink);

            if (quiz == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "QuizLink is Invalid!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            QuizComments quizComments = await _unitOfWork.QuizCommentsRepository.GetFirstOrDefaultAsync(u => u.QuizId == quiz.QuizId);

            if (quizComments == null)
            {
                return new()
                {
                    IsSuccess = false,
                    Message = "No Comments Found!!",
                    StatusCode = HttpStatusCode.BadRequest,
                };
            }

            return new()
            {
                IsSuccess = true,
                Message = "Quiz Comments Fetched Successfully!!",
                Data = quiz,
                StatusCode = HttpStatusCode.OK,
            };
        }
    }
}
