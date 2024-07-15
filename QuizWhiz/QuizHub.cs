using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Application.DTOs.Response;
using QuizWhiz.Domain.Entities;
using System.Net;
using System.Net.WebSockets;

public class QuizHub : Hub
{
    public async Task SendMessage( string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
    public async Task SendAnswer(string user,int ans)
    {
        await Clients.All.SendAsync("ReceiveResponse", true);
    }
}
