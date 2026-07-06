using Quiz.DataAccess.Models;

namespace Quiz.DataAccess.Services
{
    public interface IQuizzesService
    {
        Task<(IReadOnlyCollection<Models.Quiz> publishedQuizzes, int totalCount, int pageSize)> GetPublishedQuizzesAsync(int page);
        Task<Models.Quiz> GetQuizByIdAsync(int quizId,string userName, int pin);
        Task<Models.Quiz> CreateQuizAsync(Models.Quiz quiz);
        Task<int> PublishQuizAsync(int quizId);
        Task<Models.Quiz> JoinQuizAsync(int quizId, int quizPin, string userName);
        Task StartQuizAsync(int quizId);
        Task<Question> GetActiveQuestionAsync(int quizId, string userName, int pin);
        Task CloseActiveQuestionAsync(int quizId);
        Task NextQuestionAsync(int quizId);
        Task<int> ShowAnswer(int quizId, string userName, int pin);
        Task SendMessage(int quizId, string userName, int pin, string msg);
        Task<Models.Quiz> GetMyQuiz(int quizId);
    }
}
