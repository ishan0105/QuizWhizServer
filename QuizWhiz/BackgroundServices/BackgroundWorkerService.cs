
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuizWhiz.Application.Interface;
using QuizWhiz.DataAccess.Interfaces;
using QuizWhiz.Domain.Entities;

public class BackgroundWorkerService : BackgroundService
{
    readonly ILogger<BackgroundWorkerService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public BackgroundWorkerService(ILogger<BackgroundWorkerService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service Started.");
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service Stopped.");
        await Task.CompletedTask;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                List<Quiz> quizzes = await _unitOfWork.QuizRepository.GetAll();

                foreach (var quiz in quizzes)
                {
                  
                    if (DateTime.Now >= quiz.ScheduledDate.AddMinutes(-5) && quiz.StatusId == 2)
                    {
                        quiz.StatusId = 3;
                    }

                    DateTime completedDateTime = quiz.ScheduledDate.AddSeconds(quiz.TotalQuestion*20+1);

                    if (DateTime.Now >= completedDateTime)
                    {
                        quiz.StatusId = 4;
                    }
                }

                await _unitOfWork.SaveAsync();
                /*_logger.LogInformation("Worker running at : {time}", DateTimeOffset.Now);*/
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
