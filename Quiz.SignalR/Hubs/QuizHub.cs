using Microsoft.AspNetCore.SignalR;

namespace Quiz.SignalR.Hubs
{
    public class QuizHub : Hub
    {
        public async Task JoinQuiz(string quizId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, quizId);
        }
    }
}
