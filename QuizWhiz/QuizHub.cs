using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Application.DTOs.Response;
using QuizWhiz.Application.Interface;
using QuizWhiz.Domain.Entities;
using System.Net;
using System.Net.WebSockets;

public class QuizHub : Hub
{
    private readonly IQuizService _quizService;
    public QuizHub(IQuizService quizService)
    {
        _quizService = quizService;
    }
    public async Task SendAnswer(string QuizLink,int QuestionId,string userName,List<int>userAnswers)
    {
        var result = await _quizService.GetCorrectAnswer(QuizLink, QuestionId, userName,userAnswers);
        await Clients.All.SendAsync($"ReceiveAnswer_{QuizLink}", result , 17);
    }
    public async Task RegisterUser(string QuizLink, string userName)
    {
        /*var result = 
        await Clients.All.SendAsync($"ReceiveAnswer_{QuizLink}", result, 17);*/
    }
    /*public async Task UpdateScore(string quizLink, string userName)
    {
       var result=await _quizService.UpdateScore(quizLink, userName);

       await Clients.All.SendAsync("ReceiveResponse", true);
    }*/
}
