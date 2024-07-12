
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuizWhiz.Application.Interface;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

public class QuizTimerBackgroundService : BackgroundService
{
    private readonly IHubContext<QuizHub> _hubContext;
    /*private readonly IQuizService _quizService;*/
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public QuizTimerBackgroundService(IHubContext<QuizHub> hubContext, IServiceScopeFactory serviceScopeFactory)
    {
        _hubContext = hubContext;
        /*_quizService = quizService;*/
        _serviceScopeFactory = serviceScopeFactory;
    }



    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var quizService = scope.ServiceProvider.GetRequiredService<IQuizService>();
                var contestTime = await quizService.GetQuizTime("0GgfW7eG");
                DateTime contestStartTime = (DateTime)contestTime.Data;
                DateTime notifyTime = contestStartTime.AddMinutes(-15);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var currentTime = DateTime.Now;
                    var remainingTime = contestStartTime - currentTime;

                    if (remainingTime <= TimeSpan.Zero)
                    {
                        break;
                    }

                    if (currentTime >= notifyTime)
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveRemainingTime", "0GgfW7eG", remainingTime);
                    }

                    await Task.Delay(1000, stoppingToken);
                }
            }

            // Check for new contests periodically (e.g., every minute)
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
