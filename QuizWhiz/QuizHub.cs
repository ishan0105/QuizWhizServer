using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QuizWhiz.Application.DTOs.Request;
using QuizWhiz.Application.DTOs.Response;
using System.Net;
using System.Net.WebSockets;

public class QuizHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
    /*[HttpPost("get-single-quiz-question")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ResponseDTO> GetSingleQuestion([FromBody] GetSingleQuestionDTO getSingleQuestionDTO)
    {
        if (!ModelState.IsValid)
        {
            return new()
            {
                IsSuccess = false,
                Message = "Something Went Wrong",
                StatusCode = HttpStatusCode.BadRequest
            };
        }
        return await _quizService.GetSingleQuestion(getSingleQuestionDTO);
    }*/
}
