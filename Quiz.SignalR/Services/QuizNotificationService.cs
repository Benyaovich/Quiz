using Microsoft.AspNetCore.SignalR;
using Quiz.SignalR.Hubs;

namespace Quiz.SignalR.Services
{
    public class QuizNotificationService(IHubContext<QuizHub> hubContext) : IQuizNotificationService
    {
        public async Task NotifyQuizStart(string quizId)
        {
            await hubContext.Clients.Group(quizId).SendAsync("QuizStarted");
        }

        public async Task NotifyActualQuestionClose(string quizId)
        {
            await hubContext.Clients.Group(quizId).SendAsync("QuestionClosed");
        }

        public async Task NotifyNextQuestion(string quizId)
        {
            await hubContext.Clients.Group(quizId).SendAsync("NextQuestion");
        }

        public async Task NotifyNewChatMessage(string quizId)
        {
            await hubContext.Clients.Group(quizId).SendAsync("NewChatMessage");
        }
    }
}
