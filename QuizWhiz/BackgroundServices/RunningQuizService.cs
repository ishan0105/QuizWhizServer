using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuizWhiz.Application.Interface;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

public class RunningQuizService : BackgroundService
{
    private readonly IHubContext<QuizHub> _hubContext;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public RunningQuizService(IHubContext<QuizHub> hubContext, IServiceScopeFactory serviceScopeFactory)
    {
        _hubContext = hubContext;
        /*_quizService = quizService;*/
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _hubContext.Clients.All.SendAsync("GetTitleOfQuiz", "Hey from bg service");
    }

    public async Task StartSendingQuizTitle(CancellationToken stoppingToken)
    {
        await _hubContext.Clients.All.SendAsync("GetTitleOfQuiz", "Hey from bg service");
    }
}

