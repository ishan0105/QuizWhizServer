using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuizWhiz.Application.Interface;
using System.Threading;
using Timer = System.Threading.Timer;
using Microsoft.AspNetCore.SignalR;
using QuizWhiz.Application.DTOs.Response;
using QuizWhiz.Domain.Entities;
using QuizWhiz.DataAccess.Interfaces;

public class QuizHandleBackgroundService : BackgroundService
{
    private readonly ILogger<QuizHandleBackgroundService> _logger;
    private readonly QuizServiceManager _quizServiceManager;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHubContext<QuizHub> _hubContext;
    private readonly string _quizLink;

    public DateTime QuizScheduleTime = DateTime.Now;
    private int TimerSeconds = 0;
    private bool Timer = true;
    private bool IsMethodRunnigFistTime = false;
    public List<GetQuestionsDTO> _questions = new List<GetQuestionsDTO>();
    public int QuestionNo = 0;

    public QuizHandleBackgroundService(ILogger<QuizHandleBackgroundService> logger, string quizLink, IServiceScopeFactory serviceScopeFactory, IHubContext<QuizHub> hubContext, QuizServiceManager quizServiceManager)
    {
        _logger = logger;
        _quizLink = quizLink;
        _serviceScopeFactory = serviceScopeFactory;
        _hubContext = hubContext;
        _quizServiceManager = quizServiceManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                if (!IsMethodRunnigFistTime)
                {
                    IsMethodRunnigFistTime = true;
                    var quizService = scope.ServiceProvider.GetRequiredService<IQuizService>();
                    var Questions = await quizService.GetAllQuestions(_quizLink);
                    var ContestTime = await quizService.GetQuizTime(_quizLink);
                    DateTime ContestStartTime = (DateTime)ContestTime.Data;
                    QuizScheduleTime = ContestStartTime;

                    if (QuizScheduleTime <= DateTime.Now)
                    {
                        Timer = false;
                        var CurrentQuiz = DateTime.Now - ContestStartTime;
                        double QuizNo = Math.Ceiling(CurrentQuiz.Seconds * 1.0 / 20.0);
                        QuestionNo = (int)QuizNo - 1;
                        var CurrentSecond = CurrentQuiz.Seconds % 20;
                        TimerSeconds = CurrentSecond == 0 ? 20 : CurrentSecond;
                        --TimerSeconds;
                    }
                    else
                    {
                        var RemainingTimerSeconds = QuizScheduleTime - DateTime.Now;
                        TimerSeconds = 300 - (int)RemainingTimerSeconds.TotalSeconds;
                        --TimerSeconds;
                    }
                    if (Questions != null)
                    {
                        _questions = Questions.Data as List<GetQuestionsDTO>;
                    }
                }
                QuizHandleMethod(null);
                /*_timer = new Timer(QuizHandleMethod, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1));*/
                await Task.Delay(1000, stoppingToken);
            }
        }
        /*_logger.LogInformation("Quiz {QuizId} BackgroundService is stopping.", _quizId);*/
    }

    private async void QuizHandleMethod(object state)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            ++TimerSeconds;
            _logger.LogInformation($"Timed Hosted Service is working. Timer seconds: {TimerSeconds}");
            if (TimerSeconds > 300 && Timer)
            {
                Timer = false;
                TimerSeconds = 1;
            }
            if (Timer)
            {
                DateTime notifyTime = QuizScheduleTime.AddSeconds(-300);
                var currentTime = DateTime.Now;
                var remainingTime = QuizScheduleTime - currentTime;
                var remainingMinutes = remainingTime.Minutes.ToString("00");
                var remainingSeconds = remainingTime.Seconds.ToString("00");
                if (currentTime >= notifyTime)
                {
                    await _hubContext.Clients.All.SendAsync($"ReceiveRemainingTime_{_quizLink}", remainingMinutes, remainingSeconds);
                }
            }
            else
            {
                
                var quizService = scope.ServiceProvider.GetRequiredService<IQuizService>();
                var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                if (QuestionNo >= _questions.Count || QuestionNo < 0)
                {
                    var quiz = await quizService.GetQuiz(_quizLink);
                    var quizData = quiz.Data as Quiz;
                    if (quizData == null)
                    {
                        await _quizServiceManager.StopQuizService(_quizLink);
                        return;
                    }
                    quizData.StatusId = 4;
                    await quizService.UpdateLeaderBoard(quizData.QuizId);
                    await _unitOfWork.SaveAsync();
                    await _quizServiceManager.StopQuizService(_quizLink);
                    return;
                }

                var Question = _questions.ElementAt(QuestionNo);
                var CorrectAnswer = await quizService.GetCorrectAnswer(Question.QuestionId);
                var disqualifiedUsers = await quizService.GetDisqualifiedUsers(_quizLink);
                List<string> options = new List<string>();
                if (Question == null)
                {
                    await _quizServiceManager.StopQuizService(_quizLink);
                    return;
                }
                foreach (var ele in Question.Options)
                {
                    options.Add(ele.OptionText!.ToString());
                }

                SendQuestionDTO sendQuestionDTO = new SendQuestionDTO()
                {
                    Question = Question.Question,
                    Options = options
                };

                if (TimerSeconds == 1)
                {
                    await _hubContext.Clients.All.SendAsync($"ReceiveQuestion_{_quizLink}", QuestionNo + 1, sendQuestionDTO, TimerSeconds, disqualifiedUsers);
                }
                else if (TimerSeconds == 17)
                {
                    await _hubContext.Clients.All.SendAsync($"ReceiveAnswer_{_quizLink}", QuestionNo + 1, CorrectAnswer.Data, TimerSeconds);
                }
                else if ((TimerSeconds > 1 && TimerSeconds < 17) || TimerSeconds > 17)
                {
                    await _hubContext.Clients.All.SendAsync($"ReceiveTimerSeconds{_quizLink}", TimerSeconds);
                    if (TimerSeconds == 20)
                    {
                        TimerSeconds = 0;
                        ++QuestionNo;
                    }
                }
            }
        }
    }
}
