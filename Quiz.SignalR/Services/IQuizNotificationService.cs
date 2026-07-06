namespace Quiz.SignalR.Services
{
    public interface IQuizNotificationService
    {
        Task NotifyQuizStart(string quizId);
        Task NotifyActualQuestionClose(string quizId);
        Task NotifyNextQuestion(string quizId);
        Task NotifyNewChatMessage(string quizId);
    }
}
