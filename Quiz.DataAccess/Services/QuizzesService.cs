using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quiz.DataAccess.Config;
using Quiz.DataAccess.Exceptions;
using Quiz.DataAccess.Models;

namespace Quiz.DataAccess.Services
{
    public class QuizzesService(
        QuizDbContext context,
        IUsersService userService,
        IOptions<BigDataSettings> bigDataSettings)
        : IQuizzesService
    {
        private readonly BigDataSettings _bigDataSettings = bigDataSettings.Value;

        public async Task<(IReadOnlyCollection<Models.Quiz> publishedQuizzes,int totalCount,int pageSize)> GetPublishedQuizzesAsync(int page)
        {
            page = Math.Max(page, 1);
            var pageSize = _bigDataSettings.PageSize;

            var query = context.Quizzes
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.IsPublished);

            var totalCount = await query.CountAsync();

            var publishedQuizzes = await query
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (publishedQuizzes, totalCount,pageSize);
        }

        public async Task<Models.Quiz> GetQuizByIdAsync(int quizId, string userName, int pin)
        {
            var quiz = await GetQuizByIdAsync(quizId) ?? throw new EntityNotFoundException(nameof(Quiz));
            if (!quiz.IsPublished) throw new InvalidOperationException("The quiz is not published yet");
            if (quiz.Pin != pin) throw new InvalidOperationException("Invalid pin");
            if (!quiz.Participants.Contains(userName)) throw new InvalidOperationException("You have to join the quiz");
            return quiz;
        }

        private async Task<Models.Quiz> GetQuizByIdAsync(int quizId)
        {
            var quiz = await context.Quizzes.Include(x=>x.User).Include(x=>x.Questions).FirstOrDefaultAsync(x=>x.Id == quizId) ?? throw new EntityNotFoundException(nameof(Quiz));
            return quiz;
        }

        public async Task<Models.Quiz> CreateQuizAsync(Models.Quiz quiz)
        {
            quiz.User = await userService.GetCurrentUserAsync() ?? throw new InvalidOperationException($"You have to login");
            var newQuiz = await context.Quizzes.AddAsync(quiz);
            await context.SaveChangesAsync();
            return newQuiz.Entity;
        }

        public async Task<int> PublishQuizAsync(int quizId)
        {
            var user = await userService.GetCurrentUserAsync() ?? throw new InvalidOperationException("You have to login");
            var quiz = await GetQuizByIdAsync(quizId);
            
            if (quiz.User.Id != user.Id) throw new InvalidOperationException("You do not have permission");
            if (quiz.IsPublished) throw new InvalidOperationException("The quiz has already published");

            quiz.IsPublished = true;
            int pin = Random.Shared.Next(0, 1000000);
            quiz.Pin = pin;
            await context.SaveChangesAsync();
            return pin;
        }

        public async Task<Models.Quiz> JoinQuizAsync(int quizId, int quizPin, string userName)
        {
            var quiz = await GetQuizByIdAsync(quizId) ?? throw new EntityNotFoundException(nameof(Quiz));
            if (!quiz.IsPublished) throw new InvalidOperationException("The quiz has not published yet");
            if (quiz.IsStarted) throw new InvalidOperationException("The quiz has already started");
            if (quiz.Pin != quizPin) throw new InvalidOperationException("The pin is invalid");
            if (quiz.Participants.Contains(userName)) throw new InvalidOperationException("This username is already in use");

            quiz.Participants.Add(userName);
            await context.SaveChangesAsync();
            return quiz;
        }

        public async Task StartQuizAsync(int quizId)
        {
            var user = await userService.GetCurrentUserAsync() ?? throw new InvalidOperationException("You have to login");
            var quiz = await GetQuizByIdAsync(quizId) ?? throw new EntityNotFoundException(nameof(Quiz));

            if (quiz.User.Id != user.Id) throw new InvalidOperationException("You do not have permission");
            if (!quiz.IsPublished) throw new InvalidOperationException("The quiz has not published yet");
            if (quiz.IsStarted) throw new InvalidOperationException("The quiz has already started");

            quiz.IsStarted = true;
            await context.SaveChangesAsync();
        }

        public async Task<Question> GetActiveQuestionAsync(int quizId, string userName, int pin)
        {
            var quiz = await GetQuizByIdAsync(quizId) ?? throw new EntityNotFoundException(nameof(Quiz));
            if (!quiz.IsPublished) throw new InvalidOperationException("The quiz is not published yet");
            if (quiz.Pin != pin) throw new InvalidOperationException("Invalid pin");
            if (!quiz.Participants.Contains(userName)) throw new InvalidOperationException("You have to join the quiz");
            if (!quiz.IsStarted) throw new InvalidOperationException("The quiz is not started yet");

            return quiz.Questions.ElementAt(quiz.IndexOfActiveQuestion);
        }   

        public async Task CloseActiveQuestionAsync(int quizId)
        {
            var user = await userService.GetCurrentUserAsync() ?? throw new InvalidOperationException("You have to login");
            var quiz = await GetQuizByIdAsync(quizId) ?? throw new EntityNotFoundException(nameof(Quiz));

            if (quiz.User.Id != user.Id) throw new InvalidOperationException("You do not have permission");
            if (!quiz.IsPublished) throw new InvalidOperationException("The quiz is not published yet");
            if (!quiz.IsStarted) throw new InvalidOperationException("The quiz is not started yet");
            if (quiz.Questions.ElementAt(quiz.IndexOfActiveQuestion).IsClosed) throw new InvalidOperationException("The question is already closed");

            quiz.Questions.ElementAt(quiz.IndexOfActiveQuestion).IsClosed = true;
            await context.SaveChangesAsync();
        }

        public async Task NextQuestionAsync(int quizId)
        {
            var user = await userService.GetCurrentUserAsync() ?? throw new InvalidOperationException("You have to login");
            var quiz = await GetQuizByIdAsync(quizId) ?? throw new EntityNotFoundException(nameof(Quiz));

            if (quiz.User.Id != user.Id) throw new InvalidOperationException("You do not have permission");
            if (!quiz.IsPublished) throw new InvalidOperationException("The quiz is not published yet");
            if (!quiz.IsStarted) throw new InvalidOperationException("The quiz is not started yet");
            if (!quiz.Questions.ElementAt(quiz.IndexOfActiveQuestion).IsClosed) throw new InvalidOperationException("The question is not closed yet");

            if (quiz.IndexOfActiveQuestion + 1 >= quiz.Questions.Count)
            {
                await EndOfQuizAsync(quiz);
            }
            else
            {
                quiz.IndexOfActiveQuestion++;
                await context.SaveChangesAsync();
            }
        }

        public async Task<int> ShowAnswer(int quizId, string userName, int pin)
        {
            var quiz = await GetQuizByIdAsync(quizId) ?? throw new EntityNotFoundException(nameof(Quiz));
            if (!quiz.IsPublished) throw new InvalidOperationException("The quiz is not published yet");
            if (quiz.Pin != pin) throw new InvalidOperationException("Invalid pin");
            if (!quiz.Participants.Contains(userName)) throw new InvalidOperationException("You have to join the quiz");
            if (!quiz.IsStarted) throw new InvalidOperationException("The quiz is not started yet");
            if (!quiz.Questions.ElementAt(quiz.IndexOfActiveQuestion).IsClosed) throw new InvalidOperationException("The question is not closed yet");

            return quiz.Questions.ElementAt(quiz.IndexOfActiveQuestion).IndexOfCorrectAnswer;
        }

        private async Task EndOfQuizAsync(Models.Quiz quiz)
        {
            quiz.Pin = null;
            quiz.IsPublished = false;
            quiz.IndexOfActiveQuestion = 0;
            quiz.Participants.Clear();
            quiz.IsStarted = false;
            quiz.Chat.Clear();
            foreach (Question question in quiz.Questions)
            {
                question.IsClosed = false;
            }
            await context.SaveChangesAsync();
        }

        public async Task SendMessage(int quizId, string userName, int pin, string msg)
        {
            var quiz = await context.Quizzes.Include(x => x.Questions).FirstOrDefaultAsync(x => x.Id == quizId) ?? throw new EntityNotFoundException(nameof(Quiz));
            if (!quiz.IsPublished) throw new InvalidOperationException("The quiz is not published yet");
            if (quiz.Pin != pin) throw new InvalidOperationException("Invalid pin");
            if (!quiz.Participants.Contains(userName)) throw new InvalidOperationException("You have to join the quiz");

            quiz.Chat.Add($"{userName}: {msg}");
            await context.SaveChangesAsync();
        }

        public async Task<Models.Quiz> GetMyQuiz(int quizId)
        {
            var user = await userService.GetCurrentUserAsync() ?? throw new InvalidOperationException("You have to login");
            var quiz = await GetQuizByIdAsync(quizId) ?? throw new EntityNotFoundException(nameof(Quiz));

            if (quiz.User.Id != user.Id) throw new InvalidOperationException("You do not have permission");
            return quiz;
        }
    }
}
