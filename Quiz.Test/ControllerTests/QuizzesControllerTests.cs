using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Quiz.API.Controllers;
using Quiz.API.Infrastructure;
using Quiz.DataAccess.Models;
using Quiz.DataAccess.Services;
using Quiz.Shared.Requests;
using Quiz.Shared.Responses;
using Quiz.SignalR.Services;

namespace Quiz.Test.ControllerTests;

[TestClass]
public class QuizzesControllerTests
{
    private QuizzesController _quizzesController = null!;
    private Mock<IQuizzesService> _mockQuizzesService = null!;
    private Mock<IQuizNotificationService> _mockQuizNotificationService = null!;
    private IMapper _mapper = null!;
    
    [TestInitialize]
    public void Initialize()
    {
        var mappingConfig = new MapperConfiguration(cfg =>
            cfg.AddProfile(new MappingProfile()));

        _mapper = mappingConfig.CreateMapper();

        _mockQuizzesService = new Mock<IQuizzesService>();
        _mockQuizNotificationService = new Mock<IQuizNotificationService>();

        _quizzesController = new QuizzesController(
            _mapper,
            _mockQuizzesService.Object,
            _mockQuizNotificationService.Object);
    }
    
    [TestMethod]
    public async Task GetPublishedQuizzes()
    {
        var quizzes = new List<DataAccess.Models.Quiz>
        {
            new()
            {
                Id = 1,
                Title = "Quiz 1",
                IsStarted = false,
                User = new User { UserName = "Author1" }
            },
            new()
            {
                Id = 2,
                Title = "Quiz 2",
                IsStarted = true,
                User = new User { UserName = "Author2" }
            }
        };

        _mockQuizzesService
            .Setup(x => x.GetPublishedQuizzesAsync(1))
            .ReturnsAsync((quizzes, 2, 10));

        var result = await _quizzesController.GetPublishedQuizzes();
        Assert.IsInstanceOfType<OkObjectResult>(result);
        var obj = result as OkObjectResult;
        Assert.IsNotNull(obj);
        
        Assert.AreEqual(StatusCodes.Status200OK, obj.StatusCode);
        Assert.IsInstanceOfType<PagedQuizzesResponseDto>(obj.Value);
        var response = obj.Value as PagedQuizzesResponseDto;
        Assert.IsNotNull(response);
        
        Assert.AreEqual("Author1",response.Quizzes.First().Author);
    }
    
    [TestMethod]
    public async Task GetQuizById()
    {
        var request = new QuizAccessRequestDto
        {
            UserName = "Participant 1",
            Pin = "123456"
        };

        var quiz = new DataAccess.Models.Quiz
        {
            Id = 1,
            Title = "Test Quiz",
            IsStarted = false,
            Pin = 123456,
            User = new User { UserName = "Author" },
            Participants = ["Participant 1"]
        };

        _mockQuizzesService
            .Setup(x => x.GetQuizByIdAsync(1, request.UserName, 123456))
            .ReturnsAsync(quiz);

        var result = await _quizzesController.GetQuizById(1, request);

        _mockQuizzesService.Verify(x =>
                x.GetQuizByIdAsync(1, request.UserName, 123456),
            Times.Once);

        Assert.IsInstanceOfType<OkObjectResult>(result);
        var obj = result as OkObjectResult;
        Assert.IsNotNull(obj);
        Assert.AreEqual(StatusCodes.Status200OK, obj.StatusCode);

        Assert.IsInstanceOfType<JoinResponseDto>(obj.Value);
        var response = obj.Value as JoinResponseDto;
        Assert.IsNotNull(response);
        
        Assert.AreEqual("Author",response.Author);
    }
    
    
    [TestMethod]
    public async Task CreateQuiz()
    {
        var request = new QuizRequestDto
        {
            Title = "Test Quiz",
            Questions =
            [
                new QuestionRequestDto
                {
                    QuestionText = "Test Question",
                    Answers = ["A", "B"],
                    IndexOfCorrectAnswer = 0
                }
            ]
        };

        var createdQuiz = new DataAccess.Models.Quiz
        {
            Id = 1,
            Title = request.Title,
            IsStarted = false,
            User = new User { UserName = "Test Author" }
        };

        _mockQuizzesService
            .Setup(x => x.CreateQuizAsync(It.IsAny<DataAccess.Models.Quiz>()))
            .ReturnsAsync(createdQuiz);

        var result = await _quizzesController.CreateQuiz(request);

        _mockQuizzesService.Verify(x =>
                x.CreateQuizAsync(It.Is<DataAccess.Models.Quiz>(q =>
                    q.Title == request.Title &&
                    q.Questions.Count == request.Questions.Count)),
            Times.Once);

        Assert.IsInstanceOfType<CreatedAtActionResult>(result);
        var obj = result as CreatedAtActionResult;
        Assert.IsNotNull(obj);
        Assert.AreEqual(StatusCodes.Status201Created, obj.StatusCode);

        Assert.IsInstanceOfType<QuizResponseDto>(obj.Value);
        var response = obj.Value as QuizResponseDto;
        Assert.IsNotNull(response);
        
        Assert.AreEqual(createdQuiz.User.UserName, response.Author);
    }
    
    [TestMethod]
    public async Task PublishQuiz()
    {
        _mockQuizzesService
            .Setup(x => x.PublishQuizAsync(1))
            .ReturnsAsync(42);

        var result = await _quizzesController.PublishQuiz(1);
        _mockQuizzesService.Verify(x => x.PublishQuizAsync(1), Times.Once);

        Assert.IsInstanceOfType<OkObjectResult>(result);
        var obj = result as OkObjectResult;
        Assert.IsNotNull(obj);
        Assert.AreEqual(StatusCodes.Status200OK, obj.StatusCode);

        Assert.IsInstanceOfType<string>(obj.Value);
        var pin = obj.Value as string;
        Assert.IsNotNull(pin);
        
        Assert.AreEqual("000042", pin);
    }
    
    [TestMethod]
    public async Task JoinQuiz()
    {
        var request = new JoinRequestDto
        {
            QuizAccess = new QuizAccessRequestDto
            {
                UserName = "Participant 2",
                Pin = "123456"
            }
        };

        var quiz = new DataAccess.Models.Quiz
        {
            Id = 1,
            Title = "Test Quiz",
            IsStarted = false,
            Pin = 123456,
            User = new User { UserName = "Author" },
            Participants = ["Participant 1", "Participant 2"]
        };

        _mockQuizzesService
            .Setup(x => x.JoinQuizAsync(1, 123456, request.QuizAccess.UserName))
            .ReturnsAsync(quiz);
        
        var result = await _quizzesController.JoinQuiz(1, request);
        
        _mockQuizzesService.Verify(x =>
                x.JoinQuizAsync(1, 123456, request.QuizAccess.UserName),
            Times.Once);

        Assert.IsInstanceOfType<OkObjectResult>(result);
        var obj = result as OkObjectResult;
        Assert.IsNotNull(obj);
        Assert.AreEqual(StatusCodes.Status200OK, obj.StatusCode);

        Assert.IsInstanceOfType<JoinResponseDto>(obj.Value);
        var response = obj.Value as JoinResponseDto;
        Assert.IsNotNull(response);
        
        Assert.AreEqual(quiz.User.UserName, response.Author);
    }
    
    [TestMethod]
    public async Task StartQuiz()
    {
        _mockQuizzesService
            .Setup(x => x.StartQuizAsync(1))
            .Returns(Task.CompletedTask);

        _mockQuizNotificationService
            .Setup(x => x.NotifyQuizStart("1"))
            .Returns(Task.CompletedTask);
        
        var result = await _quizzesController.StartQuiz(1);
        
        _mockQuizzesService.Verify(x => x.StartQuizAsync(1), Times.Once);
        _mockQuizNotificationService.Verify(x => x.NotifyQuizStart("1"), Times.Once);

        Assert.IsInstanceOfType<OkResult>(result);
        var obj = result as OkResult;
        Assert.IsNotNull(obj);
        Assert.AreEqual(StatusCodes.Status200OK,obj.StatusCode);
    }
    
    [TestMethod]
    public async Task GetActiveQuestion()
    {
        var request = new QuizAccessRequestDto
        {
            UserName = "Participant 1",
            Pin = "123456"
        };

        var question = new Question
        {
            Id = 1,
            QuestionText = "Test question?",
            Answers = ["A", "B"],
            IndexOfCorrectAnswer = 0
        };

        _mockQuizzesService
            .Setup(x => x.GetActiveQuestionAsync(1, request.UserName, 123456))
            .ReturnsAsync(question);
        
        var result = await _quizzesController.GetActiveQuestion(1, request);
        
        _mockQuizzesService.Verify(x =>
                x.GetActiveQuestionAsync(1, request.UserName, 123456),
            Times.Once);

        Assert.IsInstanceOfType<OkObjectResult>(result);
        var obj = result as OkObjectResult;
        Assert.IsNotNull(obj);
        Assert.AreEqual(StatusCodes.Status200OK, obj.StatusCode);

        Assert.IsInstanceOfType<QuestionResponseDto>(obj.Value);
        var response = obj.Value as QuestionResponseDto;
        Assert.IsNotNull(response);
        
    }
    
    [TestMethod]
    public async Task NextQuestion()
    {
        _mockQuizzesService
            .Setup(x => x.NextQuestionAsync(1))
            .Returns(Task.CompletedTask);

        _mockQuizNotificationService
            .Setup(x => x.NotifyNextQuestion("1"))
            .Returns(Task.CompletedTask);

        var result = await _quizzesController.NextQuestion(1);

        _mockQuizzesService.Verify(x => x.NextQuestionAsync(1), Times.Once);

        _mockQuizNotificationService.Verify(x =>
                x.NotifyNextQuestion("1"),
            Times.Once);

        Assert.IsInstanceOfType<OkResult>(result);
        var obj = result as OkResult;
        Assert.IsNotNull(obj);
        Assert.AreEqual(StatusCodes.Status200OK, obj.StatusCode);
    }
    
    [TestMethod]
    public async Task CloseQuestion()
    {
        _mockQuizzesService
            .Setup(x => x.CloseActiveQuestionAsync(1))
            .Returns(Task.CompletedTask);

        _mockQuizNotificationService
            .Setup(x => x.NotifyActualQuestionClose("1"))
            .Returns(Task.CompletedTask);
        
        var result = await _quizzesController.CloseQuestion(1);
        
        _mockQuizzesService.Verify(x => x.CloseActiveQuestionAsync(1), Times.Once);

        _mockQuizNotificationService.Verify(x =>
                x.NotifyActualQuestionClose("1"),
            Times.Once);

        Assert.IsInstanceOfType<OkResult>(result);
        var obj = result as OkResult;
        Assert.IsNotNull(obj);
        Assert.AreEqual(StatusCodes.Status200OK, obj.StatusCode);
    }
    
    [TestMethod]
    public async Task ShowAnswer()
    {
        var request = new QuizAccessRequestDto
        {
            UserName = "Participant 1",
            Pin = "123456"
        };

        _mockQuizzesService
            .Setup(x => x.ShowAnswer(1, request.UserName, 123456))
            .ReturnsAsync(2);
        
        var result = await _quizzesController.ShowAnswer(1, request);
        
        _mockQuizzesService.Verify(x =>
                x.ShowAnswer(1, request.UserName, 123456),
            Times.Once);

        Assert.IsInstanceOfType<OkObjectResult>(result);
        var obj = result as OkObjectResult;
        Assert.IsNotNull(obj);
        Assert.AreEqual(StatusCodes.Status200OK, obj.StatusCode);

        Assert.IsInstanceOfType<int>(obj.Value);
    }
    
    [TestMethod]
    public async Task SendMessage()
    {
        var request = new ChatRequestDto
        {
            Message = "Hello",
            QuizAccess = new QuizAccessRequestDto
            {
                UserName = "Participant 1",
                Pin = "123456"
            }
        };

        _mockQuizzesService
            .Setup(x => x.SendMessage(1, request.QuizAccess.UserName, 123456, request.Message))
            .Returns(Task.CompletedTask);

        _mockQuizNotificationService
            .Setup(x => x.NotifyNewChatMessage("1"))
            .Returns(Task.CompletedTask);
        
        var result = await _quizzesController.SendMessage(1, request);
        
        _mockQuizzesService.Verify(x =>
                x.SendMessage(1, request.QuizAccess.UserName, 123456, request.Message),
            Times.Once);

        _mockQuizNotificationService.Verify(x =>
                x.NotifyNewChatMessage("1"),
            Times.Once);

        Assert.IsInstanceOfType<OkResult>(result);
        var obj = result as OkResult;
        Assert.IsNotNull(obj);
        Assert.AreEqual(StatusCodes.Status200OK, obj.StatusCode);
    }
    
    [TestMethod]
    public async Task GetMyQuiz()
    {
        var quiz = new DataAccess.Models.Quiz
        {
            Id = 1,
            Title = "Test Quiz",
            IsStarted = false,
            Pin = 123456,
            User = new User
            {
                UserName = "Author"
            },
            Questions = new List<Question>()
        };

        _mockQuizzesService
            .Setup(x => x.GetMyQuiz(1))
            .ReturnsAsync(quiz);
        
        var result = await _quizzesController.GetMyQuiz(1);
        
        _mockQuizzesService.Verify(x => x.GetMyQuiz(1), Times.Once);

        Assert.IsInstanceOfType<OkObjectResult>(result);
        var obj = result as OkObjectResult;
        Assert.IsNotNull(obj);
        Assert.AreEqual(StatusCodes.Status200OK, obj.StatusCode);

        Assert.IsInstanceOfType<ExtendedQuizResponseDto>(obj.Value);
        var response = obj.Value as ExtendedQuizResponseDto;
        Assert.IsNotNull(response);
        
        Assert.AreEqual("123456", response.Pin);
    }
}