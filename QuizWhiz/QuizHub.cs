using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Application.DTOs.Response;
using QuizWhiz.Application.Interface;
using QuizWhiz.Domain.Entities;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;

public class QuizHub : Hub
{
    private readonly IQuizService _quizService;
    private static readonly ConcurrentDictionary<string, List<string>> _userConnections = new ConcurrentDictionary<string, List<string>>();
    public QuizHub(IQuizService quizService)
    {
        _quizService = quizService;
    }
   
    public override Task OnConnectedAsync()
    {
        var username = Context.GetHttpContext().Request.Query["username"];
        if (!string.IsNullOrEmpty(username))
        {
            _quizService.AddUser(Context.ConnectionId, username);
        }
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        _quizService.RemoveUser(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
    public async Task UpdateScore(string quizLink, string userName, int QuestionId, List<int> userAnswers)
    {
        var result = await _quizService.UpdateScore(quizLink, userName, QuestionId, userAnswers);
        await Clients.All.SendAsync("ResponseOfUpdateScore", result);
    }

    public async Task RegisterUser(string quizLink, string username)
    {
        _userConnections.AddOrUpdate(
                username,
                new List<string> { Context.ConnectionId },
                (key, existingList) =>
                {
                    existingList.Add(Context.ConnectionId);
                    return existingList;
                });
        _quizService.AddUser(Context.ConnectionId, username);
        var result = await _quizService.RegisterUser(quizLink, username);
        await Clients.All.SendAsync($"RegisterUserResponse_{username}", result);
    }

    public async Task UserScoreboard(string quizLink, string username)
    {
        var result = await _quizService.GetUserScoreboard(quizLink, username);
        await Clients.All.SendAsync($"ResponseOfUserScoreboard_{username}", result);
    }

    public async Task HeartLifeline(string quizLink, string username)
    {
        var result = await _quizService.UnableHeartLifeline(quizLink, username);
        await Clients.All.SendAsync($"ResponseOfHeartLifeline_{username}", result);
    }
}