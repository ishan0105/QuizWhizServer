using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class QuizServiceManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, IHostedService> _activeQuizServices = new();

    public QuizServiceManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void StartQuizService(string QuizLink)
    {
         if(_activeQuizServices.ContainsKey(QuizLink))
            return;

         var quizService = new QuizHandleBackgroundService(
            _serviceProvider.GetRequiredService<ILogger<QuizHandleBackgroundService>>(),
            QuizLink, _serviceProvider.GetRequiredService<IServiceScopeFactory>(), _serviceProvider.GetRequiredService<IHubContext<QuizHub>>(), _serviceProvider.GetRequiredService<QuizServiceManager>());
        
        _activeQuizServices.TryAdd(QuizLink, quizService);
        _serviceProvider.GetRequiredService<IHostApplicationLifetime>().ApplicationStarted.Register(() =>
        {
            var task = quizService.StartAsync(default);
        });
    }

    public async Task StopQuizService(string QuizLink)
    {
        if (_activeQuizServices.ContainsKey(QuizLink))
        {
            if (_activeQuizServices.TryRemove(QuizLink, out var quizService))
            {
                await quizService.StopAsync(default);
            }
        }
    }
}
