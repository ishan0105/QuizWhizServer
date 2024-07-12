using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using QuizWhiz.Application.Interface;
using QuizWhiz.Application.DTOs.Request;
using System.Timers;

public class QuizHub : Hub
{
    private readonly IQuizService _quizService;
    private static System.Timers.Timer _timer;
    private static bool _timerInitialized = false;
    private static object _lock = new object();

    public QuizHub(IQuizService quizService)
    {
        _quizService = quizService;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        Console.WriteLine($"Client connected: {Context.ConnectionId}");

        lock (_lock)
        {
            if (!_timerInitialized)
            {
                _timer = new System.Timers.Timer(5000); // 5 seconds interval
                _timer.Elapsed += TimerElapsed;
                _timer.Start();
                _timerInitialized = true;
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await base.OnDisconnectedAsync(exception);
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");

        lock (_lock)
        {
            if (Clients.All == null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timerInitialized = false;
            }
        }
    }

    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        // Send a message to all connected clients to fetch the next question
        Clients.All.SendAsync("FetchNextQuestion");
    }

    public async Task GetNewQuestion(string quizLink, int questionCount)
    {
        GetSingleQuestionDTO getSingleQuestionDTO = new GetSingleQuestionDTO
        {
            QuizLink = quizLink,
            QuestionCount = questionCount
        };

        var response = await _quizService.GetSingleQuestion(getSingleQuestionDTO);
        if (response.IsSuccess)
        {
            await Clients.Caller.SendAsync("ReceiveQuestion", response.Data);
        }
        else
        {
            response.Message = "Question does not exists";
            await Clients.Caller.SendAsync("ReceiveError", response.Message);
        }
    }

    public async Task GetCorrectAnswer(string quizLink, int questionCount)
    {
        try
        {
            var response = await _quizService.GetCorrectAnswer(quizLink, questionCount);
            if (response.IsSuccess)
            {
                await Clients.Caller.SendAsync("ReceiveCorrectAnswer", response.Data);
            }
            else
            {
                response.Message = "Correct answer not found";
                await Clients.Caller.SendAsync("ReceiveError", response.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetCorrectAnswer: {ex.Message}");
            await Clients.Caller.SendAsync("ReceiveError", "An error occurred on the server.");
        }
    }
}
