using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Application.DTOs.Response;
using QuizWhiz.Application.Interface;
using QuizWhiz.Application.Services;
using QuizWhiz.Domain.Entities;
using System.Net;
using System.Net.WebSockets;

public class QuizHub : Hub
{
    private readonly IQuizService _quizService;
    public QuizHub(QuizService quizService)
    {
        _quizService = quizService;
    }
    public async Task SendMessage( string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
    public async Task SendAnswer(string user,int optionId)
    {
        await Clients.All.SendAsync("ReceiveResponse", true);
    }
    public async Task CheckQuizAnswer(string quizLink, string userName, bool isAns)
    {
        await _quizService.CheckQuizAnswer(quizLink, userName, isAns);  
        await Clients.All.SendAsync("ReceiveCorrectAnswerMessage", true);
    }
}
