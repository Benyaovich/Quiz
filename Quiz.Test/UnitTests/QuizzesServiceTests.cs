using System.Reflection;
using Quiz.DataAccess;
using Quiz.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Options;
using Quiz.DataAccess.Config;
using Quiz.DataAccess.Models;
using Quiz.DataAccess.Exceptions;

namespace Quiz.Test.UnitTests
{
    [TestClass]
    public class QuizzesServiceTests : IDisposable
    {

        private QuizDbContext _context = null!;
        private QuizzesService _quizzesService = null!;
        private Mock<IUsersService> _mockUsersService = null!;

        private readonly User _user = new()
        {
            UserName = "Test User",
            Email = "test@test.com",
            PasswordHash = "Fake Hash"
        };

        [TestInitialize]
        public async Task Initialize()
        {
            var options = new DbContextOptionsBuilder<QuizDbContext>()
                .UseInMemoryDatabase("TestQuizzesServiceDatabase")
                .Options;

            _context = new QuizDbContext(options);
            
            _mockUsersService = new Mock<IUsersService>();

            var bigDataSettings = Options.Create(new BigDataSettings { PageSize = 10 });

            _quizzesService = new QuizzesService(_context, _mockUsersService.Object,bigDataSettings);

            await SeedDataBase();
        }

        [TestMethod]
        [DataRow(1,10)]
        [DataRow(2,10)]
        public async Task GetPublishedQuizzes(int page,int expectedQuizzesCount)
        {
            var res =  await _quizzesService.GetPublishedQuizzesAsync(page);
            Assert.AreEqual(20, res.totalCount);
            Assert.AreEqual(10, res.pageSize);
            Assert.AreEqual(expectedQuizzesCount, res.publishedQuizzes.Count);
        }
        
        [TestMethod]
        public async Task GetQuizByIdAsync_ThrowsEntityNotFoundException_WhenQuizNotFound()
        {
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _quizzesService.GetQuizByIdAsync(-1,"Does not matter",0));
        }

        [TestMethod]
        public async Task GetQuizByIdAsync_ThrowsInvalidOperationException_WhenQuizIsNotPublishedYet()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.GetQuizByIdAsync(21,"Does not matter",0));
        }
        
        [TestMethod]
        public async Task GetQuizByIdAsync_ThrowsInvalidOperationException_WhenPinIsInvalid()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.GetQuizByIdAsync(1,"Does not matter",-1));
        }
        
        [TestMethod]
        public async Task GetQuizByIdAsync_ThrowsInvalidOperationException_WhenUserNameNotFoundInParticipants()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.GetQuizByIdAsync(1,"Username which isn't exists in participants",0));
        }

        [TestMethod]
        public async Task GetQuizByIdAsync()
        {
            var quiz = await _quizzesService.GetQuizByIdAsync(1, "Participant 1", 0);
            Assert.IsInstanceOfType<DataAccess.Models.Quiz>(quiz);
            Assert.AreEqual(1,quiz.Id);
        }

        [TestMethod]
        public async Task CreateQuizAsync_ThrowsInvalidOperationException_WhenUserNotLoggedIn()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                _quizzesService.CreateQuizAsync(new DataAccess.Models.Quiz()));
        }

        [TestMethod]
        public async Task CreateQuizAsync()
        {
            MockLogin();
            var newQuiz = new DataAccess.Models.Quiz
            {
                Title = "Created Quiz",
                Questions = new List<Question>
                {
                    new(){
                        QuestionText = "Test Question 1",
                        Answers = [
                            "Test Answer 1",
                            "Test Answer 2"
                        ],
                        IndexOfCorrectAnswer = 0
                    }
                }
            };
            var res = await _quizzesService.CreateQuizAsync(newQuiz);
            Assert.AreEqual(_user.Id,res.User.Id);
        }

        [TestMethod]
        public async Task PublishQuizAsync_ThrowsInvalidOperationException_WhenUserNotLoggedIn()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.PublishQuizAsync(1));
        }

        [TestMethod]
        public async Task PublishQuizAsync_ThrowsEntityNotFoundException_WhenQuizNotFound()
        {
            MockLogin();
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _quizzesService.PublishQuizAsync(-1));
        }
        
        [TestMethod]
        public async Task PublishQuizAsync_ThrowsInvalidOperationException_WhenUserIsNotMatching()
        {
            MockFakeLogin();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.PublishQuizAsync(1));
        }
        
        [TestMethod]
        public async Task PublishQuizAsync_ThrowsInvalidOperationException_WhenQuizIsAlreadyPublished()
        {
            MockLogin();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.PublishQuizAsync(1));
        }

        [TestMethod]
        public async Task PublishQuizAsync()
        {
            MockLogin();
            var res = await _quizzesService.PublishQuizAsync(21);
            var quiz = await _context.Quizzes.FirstAsync(x => x.Id == 21);
            Assert.IsTrue(quiz.IsPublished);
            Assert.AreEqual(quiz.Pin,res);
        }

        [TestMethod]
        public async Task JoinQuizAsync_ThrowsEntityNotFoundException_WhenQuizNotFound()
        {
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() =>
                _quizzesService.JoinQuizAsync(-1, -1, "Does not matter"));
        }
        
        [TestMethod]
        public async Task JoinQuizAsync_InvalidOperationException_WhenQuizIsNotPublishedYet()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                _quizzesService.JoinQuizAsync(21, 0, "Does not matter"));
        }
        
        [TestMethod]
        public async Task JoinQuizAsync_InvalidOperationException_WhenQuizIsAlreadyStarted()
        {
            var quiz = await _context.Quizzes.FirstAsync();
            quiz.IsStarted = true;
            await  _context.SaveChangesAsync();
            
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                _quizzesService.JoinQuizAsync(1, 0, "Does not matter"));
        }
        
        [TestMethod]
        public async Task JoinQuizAsync_InvalidOperationException_WhenPinIsInvalid()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                _quizzesService.JoinQuizAsync(2, -1, "Does not matter"));
        }
        
        [TestMethod]
        public async Task JoinQuizAsync_InvalidOperationException_WhenUserNameIsTaken()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                _quizzesService.JoinQuizAsync(1, 0, "Participant 1"));
        }
        
        [TestMethod]
        public async Task JoinQuizAsync()
        {
            var res = await _quizzesService.JoinQuizAsync(1, 0, "Participant 2");
            Assert.IsTrue(res.Participants.Contains("Participant 2"));
        }

        [TestMethod]
        public async Task StartQuizAsync_ThrowsInvalidOperationException_WhenUserNotLoggedIn()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.StartQuizAsync(1));
        }
        
        [TestMethod]
        public async Task StartQuizAsync_ThrowsEntityNotFoundException_WhenQuizNotFound()
        {
            MockLogin();
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _quizzesService.StartQuizAsync(-1));
        }
        
        [TestMethod]
        public async Task StartQuizAsync_ThrowsInvalidOperationException_WhenUserIsNotMatching()
        {
            MockFakeLogin();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.StartQuizAsync(1));
        }
        
        [TestMethod]
        public async Task StartQuizAsync_ThrowsInvalidOperationException_WhenQuizIsNotPublishedYet()
        {
            MockLogin();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.StartQuizAsync(21));
        }
        
        [TestMethod]
        public async Task StartQuizAsync_ThrowsInvalidOperationException_WhenQuizIsAlreadyStarted()
        {
            MockLogin();
            var quiz = await _context.Quizzes.FirstAsync();
            quiz.IsStarted = true;
            await _context.SaveChangesAsync();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.StartQuizAsync(1));
        }

        [TestMethod]
        public async Task StartQuizAsync()
        {
            MockLogin();
            await _quizzesService.StartQuizAsync(1);
            Assert.IsTrue((await _context.Quizzes.FirstAsync()).IsStarted);
        }

        [TestMethod]
        public async Task GetActiveQuestionAsync_ThrowsEntityNotFoundException_WhenQuizNotFound()
        {
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _quizzesService.GetActiveQuestionAsync(-1,"Does not matter",0));
        }

        [TestMethod]
        public async Task GetActiveQuestionAsync_ThrowsInvalidOperationException_WhenQuizIsNotPublishedYet()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.GetActiveQuestionAsync(21,"Does not matter",0));
        }
        
        [TestMethod]
        public async Task GetActiveQuestionAsync_ThrowsInvalidOperationException_WhenPinIsInvalid()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.GetActiveQuestionAsync(1,"Does not matter",-1));
        }
        
        [TestMethod]
        public async Task GetActiveQuestionAsync_ThrowsInvalidOperationException_WhenUserNameNotFoundInParticipants()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.GetActiveQuestionAsync(1,"Username which isn't exists in participants",0));
        }
        
        [TestMethod]
        public async Task GetActiveQuestionAsync_ThrowsInvalidOperationException_WhenQuizIsNotStartedYet()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.GetActiveQuestionAsync(1,"Participant 1",0));
        }

        [TestMethod]
        public async Task GetActiveQuestionAsync()
        {
            var quiz = await _context.Quizzes.Include(x=>x.Questions).FirstAsync();
            quiz.IsStarted = true;
            var res = await _quizzesService.GetActiveQuestionAsync(1,"Participant 1",0);
            Assert.AreSame(quiz.Questions.ElementAt(quiz.IndexOfActiveQuestion),res);
        }

        [TestMethod]
        public async Task CloseActiveQuestionAsync_ThrowsInvalidOperationException_WhenUserNotLoggedIn()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.CloseActiveQuestionAsync(1));
        }
        
        [TestMethod]
        public async Task CloseActiveQuestionAsync_ThrowsEntityNotFoundException_WhenQuizNotFound()
        {
            MockLogin();
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _quizzesService.CloseActiveQuestionAsync(-1));
        }
        
        [TestMethod]
        public async Task CloseActiveQuestionAsync_ThrowsInvalidOperationException_WhenUserIsNotMatching()
        {
            MockFakeLogin();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.CloseActiveQuestionAsync(1));
        }
        
        [TestMethod]
        public async Task CloseActiveQuestionAsync_ThrowsInvalidOperationException_WhenQuizIsNotPublishedYet()
        {
            MockLogin();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.CloseActiveQuestionAsync(21));
        }
        
        [TestMethod]
        public async Task CloseActiveQuestionAsync_ThrowsInvalidOperationException_WhenQuizIsNotStartedYet()
        {
            MockLogin();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.CloseActiveQuestionAsync(1));
        }
        
        [TestMethod]
        public async Task CloseActiveQuestionAsync_ThrowsInvalidOperationException_WhenQuestionIsAlreadyClosed()
        {
            MockLogin();
            var quiz = await _context.Quizzes.Include(x=>x.Questions).FirstAsync();
            quiz.Questions.ElementAt(quiz.IndexOfActiveQuestion).IsClosed = true;
            await _context.SaveChangesAsync();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.CloseActiveQuestionAsync(1));
        }
        
        [TestMethod]
        public async Task CloseActiveQuestionAsync()
        {
            MockLogin();
            var quiz = await _context.Quizzes.Include(x=>x.Questions).FirstAsync(x=>x.Id == 1);
            quiz.IsStarted = true;
            await _context.SaveChangesAsync();
            await _quizzesService.CloseActiveQuestionAsync(1);
            Assert.IsTrue(quiz.Questions.ElementAt(quiz.IndexOfActiveQuestion).IsClosed);
        }

        [TestMethod]
        public async Task NextQuestionAsync_ThrowsInvalidOperationException_WhenUserNotLoggedIn()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.NextQuestionAsync(1));
        }
        
        [TestMethod]
        public async Task NextQuestionAsync_ThrowsEntityNotFoundException_WhenQuizNotFound()
        {
            MockLogin();
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _quizzesService.NextQuestionAsync(-1));
        }
        
        [TestMethod]
        public async Task NextQuestionAsync_ThrowsInvalidOperationException_WhenUserIsNotMatching()
        {
            MockFakeLogin();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.NextQuestionAsync(1));
        }
        
        [TestMethod]
        public async Task NextQuestionAsync_ThrowsInvalidOperationException_WhenQuizIsNotPublishedYet()
        {
            MockLogin();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.NextQuestionAsync(21));
        }
        
        [TestMethod]
        public async Task NextQuestionAsync_ThrowsInvalidOperationException_WhenQuizIsNotStartedYet()
        {
            MockLogin();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.NextQuestionAsync(1));
        }
        
        [TestMethod]
        public async Task NextQuestionAsync_ThrowsInvalidOperationException_WhenQuestionIsNotClosedYet()
        {
            MockLogin();
            var quiz = await _context.Quizzes.Include(x => x.Questions).FirstAsync();
            quiz.IsStarted = true;
            await _context.SaveChangesAsync();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.NextQuestionAsync(1));
        }
        
        [TestMethod]
        public async Task NextQuestionAsync()
        {
            MockLogin();
            var quiz = await _context.Quizzes.Include(x => x.Questions).FirstAsync();
            quiz.IsStarted = true;
            quiz.Questions.ElementAt(quiz.IndexOfActiveQuestion).IsClosed = true;
            await _context.SaveChangesAsync();
            await _quizzesService.NextQuestionAsync(1);
            quiz = await _context.Quizzes.Include(x => x.Questions).FirstAsync();
            quiz.Questions.ElementAt(quiz.IndexOfActiveQuestion).IsClosed = true;
            Assert.AreEqual(1,quiz.IndexOfActiveQuestion);
            
            //Next question ends quiz when there is no question left
            await _quizzesService.NextQuestionAsync(1);
            quiz = await _context.Quizzes.Include(x=>x.Questions).FirstAsync();
            Assert.AreEqual(0,quiz.IndexOfActiveQuestion);
            Assert.IsFalse(quiz.IsPublished);
            Assert.IsFalse(quiz.IsPublished);
            Assert.IsNull(quiz.Pin);
            Assert.AreEqual(0,quiz.Participants.Count);
            Assert.AreEqual(0,quiz.Chat.Count);
            Assert.IsFalse(quiz.Questions.Any(x=>x.IsClosed));
        }

        [TestMethod]
        public async Task ShowAnswer_ThrowsEntityNotFoundException_WhenQuizNotFound()
        {
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() =>
                _quizzesService.ShowAnswer(-1, "Does not matter", 0));
        }
        
        [TestMethod]
        public async Task ShowAnswer_ThrowsInvalidOperationException_WhenQuizIsNotPublishedYet()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                _quizzesService.ShowAnswer(21, "Does not matter", 0));
        }
        
        [TestMethod]
        public async Task ShowAnswer_ThrowsInvalidOperationException_WhenPinIsInvalid()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                _quizzesService.ShowAnswer(1, "Does not matter", -1));
        }
        
        [TestMethod]
        public async Task ShowAnswer_ThrowsInvalidOperationException_WhenUserNameNotFoundInParticipants()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                _quizzesService.ShowAnswer(1, "Username which isn't exists in participants", 0));
        }
        
        [TestMethod]
        public async Task ShowAnswer_ThrowsInvalidOperationException_WhenQuizIsNotStartedYet()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                _quizzesService.ShowAnswer(1, "Participant 1", 0));
        }
        
        [TestMethod]
        public async Task ShowAnswer_ThrowsInvalidOperationException_WhenQuestionIsNotClosedYet()
        {
            var quiz = await _context.Quizzes.FirstAsync();
            quiz.IsStarted = true;
            await _context.SaveChangesAsync();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                _quizzesService.ShowAnswer(1, "Participant 1", 0));
        }

        [TestMethod]
        public async Task ShowAnswer()
        {
            var quiz = await _context.Quizzes.Include(x => x.Questions).FirstAsync();
            quiz.IsStarted = true;
            quiz.Questions.ElementAt(quiz.IndexOfActiveQuestion).IsClosed = true;
            var res = await _quizzesService.ShowAnswer(1,"Participant 1",0);
            Assert.AreEqual(quiz.Questions.ElementAt(quiz.IndexOfActiveQuestion).IndexOfCorrectAnswer, res);
        }

        [TestMethod]
        public async Task SendMessage_ThrowsEntityNotFoundException_WhenQuizNotFound()
        {
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _quizzesService.SendMessage(-1, "Does not matter", 0,""));
        }
        
        [TestMethod]
        public async Task SendMessage_ThrowsInvalidOperationException_WhenQuizIsNotPublishedYet()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.SendMessage(21, "Does not matter", 0,""));
        }
        
        [TestMethod]
        public async Task SendMessage_ThrowsInvalidOperationException_WhenPinIsInvalid()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.SendMessage(1, "Does not matter", -1,""));
        }
        
        [TestMethod]
        public async Task SendMessage_ThrowsInvalidOperationException_WhenUserNameIsNotFoundInParticipants()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.SendMessage(1, "Username which isn't exists in participants", 0,""));
        }
        
        [TestMethod]
        public async Task SendMessage()
        {
            await _quizzesService.SendMessage(1, "Participant 1", 0, "message");
            var quiz = await _context.Quizzes.FirstAsync();
            Assert.AreEqual("Participant 1: message",quiz.Chat.First());
        }

        [TestMethod]
        public async Task GetMyQuiz_ThrowsInvalidOperationException_WhenUserIsNotLoggedIn()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.GetMyQuiz(1));
        }
        
        [TestMethod]
        public async Task GetMyQuiz_ThrowsEntityNotFoundException_WhenQuizNotFound()
        {
            MockLogin();
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _quizzesService.GetMyQuiz(-1));
        }
        
        [TestMethod]
        public async Task GetMyQuiz_ThrowsInvalidOperationException_WhenUserIsNotMatching()
        {
            MockFakeLogin();
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _quizzesService.GetMyQuiz(1));
        }

        [TestMethod]
        public async Task GetMyQuiz()
        {
            MockLogin();
            var res = await _quizzesService.GetMyQuiz(1);
            Assert.AreEqual(_user.Id,res.User.Id);
        }
        
        private async Task SeedDataBase()
        {
            _context.Users.Add(_user);

            List<DataAccess.Models.Quiz> quizzes = new List<DataAccess.Models.Quiz>();
            for(int i = 1; i < 21; i++)
            {
                quizzes.Add(new DataAccess.Models.Quiz
                {
                    Pin = 0,
                    Title = $"Test Quiz {i}",
                    Participants = ["Participant 1"],
                    Questions = new List<Question>{
                        new(){
                            QuestionText = "Test Question 1",
                            Answers = [
                                "Test Answer 1",
                                "Test Answer 2"
                            ],
                            IndexOfCorrectAnswer = 0
                        },
                        new(){
                            QuestionText = "Test Question 2",
                            Answers = [
                                "Test Answer 1",
                                "Test Answer 2"
                            ],
                            IndexOfCorrectAnswer = 0
                        }
                    },
                    IsPublished = true,
                    IndexOfActiveQuestion = 0,
                    User = _user
                });
            }

            quizzes.Add(new DataAccess.Models.Quiz
            {
                Title = $"Test Quiz (Not Published)",
                Questions = new List<Question>{
                    new(){
                        QuestionText = "Test Question 1",
                        Answers = [
                            "Test Answer 1",
                            "Test Answer 2"
                        ],
                        IndexOfCorrectAnswer = 0
                    }
                },
                User = _user
            });

            _context.Quizzes.AddRange(quizzes);

            await _context.SaveChangesAsync();
        }

        private void MockLogin()
        {
            _mockUsersService.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync(_user);
        }

        private void MockFakeLogin()
        {
            _mockUsersService.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync(new User
            {
                Id = "Fake User Id"
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            Dispose();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
