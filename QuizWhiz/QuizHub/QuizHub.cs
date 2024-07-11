using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Npgsql;
using QuizWhiz.Application.Interface;
using QuizWhiz.Application.DTOs.Request;

public class QuizHub : Hub
{
    private readonly IQuizService _quizService;
    public QuizHub(IQuizService quizService)
    {
        _quizService = quizService;
    }

    //public async Task SendMessage(string user, string message)
    //{
    //    await Clients.All.SendAsync("ReceiveMessage", user, message);
    //}

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
