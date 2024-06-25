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

        public async Task<ResponseDTO> CreateNewQuizAsync(QuizDTO quizDTO)
        {
            var quizzes = _unitOfWork.QuizRepository.GetAll();

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
            } while(!isLinkUnique);

            var token = _jwtHelper.DecodeToken();
            var adminUsername = token.Username;

            var admin = (await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(u => u.Username == adminUsername));

            if(admin != null)
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
    }
}
