using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
public class QuestionService : IHostedService, IDisposable
{
    private readonly IHubContext<QuizHub> _hubContext;
    private readonly ILogger<QuestionService> _logger;
    private Timer _timer;
    private int _questionIndex;

    public QuestionService(IHubContext<QuizHub> hubContext, ILogger<QuestionService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
        _questionIndex = 0;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Question Service is starting.");
        _timer = new Timer(SendQuestion, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        return Task.CompletedTask;
    }

    private async void SendQuestion(object state)
    {
        var question = GetNextQuestion();
       /* _logger.LogInformation("Sending question: {Question}", question);*/
        await _hubContext.Clients.All.SendAsync("ReceiveQuestion", question);
    }

    private string GetNextQuestion()
    {
        string[] questions = new[]
        {
            "Question 1?",
            "Question 2?",
            "Question 3?",
            // Add more questions as needed
        };

        var question = questions[_questionIndex];
        _questionIndex = (_questionIndex + 1) % questions.Length;
        return question;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Question Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
