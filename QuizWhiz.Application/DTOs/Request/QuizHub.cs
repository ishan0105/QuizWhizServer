using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Application.DTOs.Response;
using System.Net;
using System.Net.WebSockets;

public class QuizHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
    public async Task SendRemainingTime(string contestId, TimeSpan remainingTime)
    {
        await Clients.All.SendAsync("ReceiveRemainingTime", contestId, remainingTime);
    }
}
