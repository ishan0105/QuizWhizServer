
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuizWhiz.Application.Interface;
using QuizWhiz.DataAccess.Interfaces;
using QuizWhiz.Domain.Entities;

public class QuizLeaderboardGenerateService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public QuizLeaderboardGenerateService( IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var quizService = scope.ServiceProvider.GetRequiredService<IQuizService>();
                List<Quiz> quizzes = await _unitOfWork.QuizRepository.GetAll();

                foreach (var quiz in quizzes)
                {

                    if (quiz!=null && quiz.StatusId==4)
                    {
                        await quizService.UpdateLeaderBoard(quiz.QuizId);
                    }
                }

                /*_logger.LogInformation("Worker running at : {time}", DateTimeOffset.Now);*/
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
