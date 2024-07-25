
using System.Buffers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Application.DTOs.Response;
using QuizWhiz.Application.Interface;
using QuizWhiz.Application.Services;
using QuizWhiz.Domain.Entities;

public class QuizStartBackgroundService : BackgroundService
{
    private readonly QuizServiceManager _quizServiceManager;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public QuizStartBackgroundService(IServiceProvider serviceProvider, IServiceScopeFactory serviceScopeFactory)
    {
        _quizServiceManager = serviceProvider.GetRequiredService<QuizServiceManager>();
        _serviceScopeFactory = serviceScopeFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var QuizService = scope.ServiceProvider.GetRequiredService<IQuizService>();
                List<KeyValuePair<int, string>>QuizLinks = QuizService.GetActiveQuizzes();
              
                foreach (var Link in QuizLinks)
                {
                    if (Link.Key == 4) await _quizServiceManager.StopQuizService(Link.Value);
                    else _quizServiceManager.StartQuizService(Link.Value);
                }

                await Task.Delay(1000, stoppingToken); 
            }
        }
    }
}


