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
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
    public async Task UpdateScore(string quizLink, string userName)
    {
       var result=await _quizService.UpdateScore(quizLink, userName);

       await Clients.All.SendAsync("ReceiveResponse", true);
    }
}
