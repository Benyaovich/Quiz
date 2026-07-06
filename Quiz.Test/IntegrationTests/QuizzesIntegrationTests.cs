using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quiz.DataAccess;
using Quiz.DataAccess.Models;
using Quiz.Shared.Requests;
using Quiz.Shared.Responses;

namespace Quiz.Test.IntegrationTests;

[TestClass]
public class QuizzesIntegrationTests
{
    private WebApplicationFactory<Program> _server = null!;
    private HttpClient _client = null!;
    
    private readonly LoginRequestDto _loginRequest = new()
    {
        Email = "test@test.com",
        Password = "Test123*"
    };

    [TestInitialize]
    public void Initialize()
    {
        _server = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("IntegrationTest");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<QuizDbContext>>();
                services.AddDbContext<QuizDbContext>(options => options.UseInMemoryDatabase("QuizzesIntegrationTests"));
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<QuizDbContext>();
                dbContext.Database.EnsureCreated();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                _ = SeedDataBase(dbContext,userManager);
            });
        });
        
        _client = _server.CreateClient();
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    [TestMethod]
    [DataRow(1,10)]
    [DataRow(2,10)]
    public async Task GetPublishedQuizzes(int page, int quizCount)
    {
        var response = await _client.GetAsync($"/quizzes?{page}");
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, content);

        var body = await response.Content.ReadFromJsonAsync<PagedQuizzesResponseDto>();
        Console.WriteLine(body);
        Assert.IsNotNull(body);
        
        Assert.AreEqual(quizCount, body.Quizzes.Count);
        Assert.AreEqual(20, body.Pagination.TotalCount);
    }
    
    [TestMethod]
    public async Task GetQuizById()
    {
        var request = new QuizAccessRequestDto
        {
            UserName = "Participant 1",
            Pin = "000000"
        };
        var response = await _client.PostAsJsonAsync($"/quizzes/1", request);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JoinResponseDto>();
        Assert.IsNotNull(body);
        Assert.AreEqual(1,body.Id); 
    }
    
    [TestMethod]
    public async Task GetQuizById_ReturnsConflict_WhenPinIsInvalid()
    {
        var request = new QuizAccessRequestDto
        {
            UserName = "Participant 1",
            Pin = "123245"
        };

        var response = await _client.PostAsJsonAsync($"/quizzes/1", request);
        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task CreateQuiz()
    {
        var loginResponse = await _client.PostAsJsonAsync("/users/login", _loginRequest);
        Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.IsNotNull(loginBody);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.AuthToken);

        var request = new QuizRequestDto
        {
            Title = "Created Quiz",
            Questions =
            [
                new QuestionRequestDto
                {
                    QuestionText = "Question 1",
                    Answers = ["A", "B"],
                    IndexOfCorrectAnswer = 0
                }
            ]
        };

        var response = await _client.PostAsJsonAsync("/quizzes/create", request);
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, content);

        var body = await response.Content.ReadFromJsonAsync<QuizResponseDto>();

        Assert.IsNotNull(body);
        Assert.AreEqual(request.Title, body.Title);
        Assert.AreEqual(22,body.Id);
    }
    
    [TestMethod]
    public async Task CreateQuiz_ReturnsUnauthorized_WhenNoToken()
    {
        var request = new QuizRequestDto
        {
            Title = "Test Quiz",
            Questions =
            [
                new QuestionRequestDto
                {
                    QuestionText = "Question 1",
                    Answers = ["A", "B"],
                    IndexOfCorrectAnswer = 0
                }
            ]
        };

        var response = await _client.PostAsJsonAsync("/quizzes/create", request);
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task PublishQuiz()
    {

        var loginResponse = await _client.PostAsJsonAsync("/users/login", _loginRequest);
        Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.IsNotNull(loginBody);
        
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.AuthToken);
        
        var response = await _client.GetAsync("/quizzes/21/publish");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<string>();
        Assert.IsNotNull(content);
        Assert.AreEqual(6,content.Length);
    }
    
    [TestMethod]
    public async Task PublishQuiz_ReturnsUnauthorized_WhenNoToken()
    {
        var response = await _client.GetAsync("/quizzes/1/publish");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    
    
    [TestMethod]
    public async Task JoinQuiz()
    {
        var request = new JoinRequestDto
        {
            QuizAccess = new QuizAccessRequestDto
            {
                UserName = "New Participant",
                Pin = "000000"
            }
        };

        var response = await _client.PostAsJsonAsync("/quizzes/1/join", request);
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, content);

        var body = await response.Content.ReadFromJsonAsync<JoinResponseDto>();
        Assert.IsNotNull(body);
        
        Assert.IsTrue(body.Participants.Contains("New Participant"));
    }
    
    [TestMethod]
    public async Task JoinQuiz_ReturnsConflict_WhenPinIsInvalid()
    {
        var request = new JoinRequestDto
        {
            QuizAccess = new QuizAccessRequestDto
            {
                UserName = "New Participant",
                Pin = "999999"
            }
        };

        var response = await _client.PostAsJsonAsync("/quizzes/1/join", request);
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode, content);
    }
    
    [TestMethod]
    public async Task JoinQuiz_ReturnsConflict_WhenUserNameAlreadyTaken()
    {
        var request = new JoinRequestDto
        {
            QuizAccess = new QuizAccessRequestDto
            {
                UserName = "Participant 1",
                Pin = "000000"
            }
        };

        var response = await _client.PostAsJsonAsync("/quizzes/1/join", request);
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode, content);
    }
    
    [TestMethod]
    public async Task StartQuiz()
    {

        var loginResponse = await _client.PostAsJsonAsync("/users/login", _loginRequest);
        Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.IsNotNull(loginBody);
        
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.AuthToken);
        
        var response = await _client.GetAsync($"/quizzes/1/start");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        
    }
    
    [TestMethod]
    public async Task StartQuiz_ReturnsUnauthorized_WhenNoToken()
    {
        var response = await _client.GetAsync("/quizzes/1/start");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task GetActiveQuestion()
    {
        var loginResponse = await _client.PostAsJsonAsync("/users/login", _loginRequest);
        Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.IsNotNull(loginBody);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.AuthToken);
        await _client.GetAsync("/quizzes/1/start");
        
        
        var request = new QuizAccessRequestDto
        {
            UserName = "Participant 1",
            Pin = "000000"
        };

        var response = await _client.PostAsJsonAsync($"/quizzes/1/activeQuestion", request);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadFromJsonAsync<QuestionResponseDto>();

        Assert.IsNotNull(body);
        Assert.AreEqual("Test Question 1", body.QuestionText);
    }
    
    [TestMethod]
    public async Task GetActiveQuestion_ReturnsConflict_WhenPinInvalid()
    {
        var request = new QuizAccessRequestDto
        {
            UserName = "Participant 1",
            Pin = "999999"
        };

        var response = await _client.PostAsJsonAsync($"/quizzes/1/activeQuestion", request);

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task NextQuestion()
    {

        var loginResponse = await _client.PostAsJsonAsync("/users/login", _loginRequest);
        Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.IsNotNull(loginBody);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.AuthToken);
        
        var startResponse = await _client.GetAsync($"/quizzes/1/start");
        Assert.AreEqual(HttpStatusCode.OK, startResponse.StatusCode);
        
        var closeResponse = await _client.GetAsync($"/quizzes/1/closeQuestion");
        Assert.AreEqual(HttpStatusCode.OK, closeResponse.StatusCode);
        
        var response = await _client.GetAsync($"/quizzes/1/nextQuestion");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
    
    [TestMethod]
    public async Task NextQuestion_ReturnsUnauthorized_WhenNoToken()
    {
        var response = await _client.GetAsync("/quizzes/1/nextQuestion");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task CloseQuestion()
    {
        var loginResponse = await _client.PostAsJsonAsync("/users/login", _loginRequest);
        Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.IsNotNull(loginBody);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.AuthToken);
        
        var startResponse = await _client.GetAsync($"/quizzes/1/start");
        Assert.AreEqual(HttpStatusCode.OK, startResponse.StatusCode);
        
        var response = await _client.GetAsync($"/quizzes/1/closeQuestion");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        
    }
    
    [TestMethod]
    public async Task CloseQuestion_ReturnsUnauthorized_WhenNoToken()
    {
        var response = await _client.GetAsync("/quizzes/1/closeQuestion");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    
    [TestMethod]
    public async Task ShowAnswer()
    {
        var loginResponse = await _client.PostAsJsonAsync("/users/login", _loginRequest);
        Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.IsNotNull(loginBody);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.AuthToken);
        
        var startResponse = await _client.GetAsync($"/quizzes/1/start");
        Assert.AreEqual(HttpStatusCode.OK, startResponse.StatusCode);
        
        var closeResponse = await _client.GetAsync($"/quizzes/1/closeQuestion");
        Assert.AreEqual(HttpStatusCode.OK, closeResponse.StatusCode);
        
        var request = new QuizAccessRequestDto
        {
            UserName = "Participant 1",
            Pin = "000000"
        };

        var response = await _client.PostAsJsonAsync(
            $"/quizzes/1/activeQuestion/answer",
            request);
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var answerIndex = await response.Content.ReadFromJsonAsync<int>();
        Assert.AreEqual(0, answerIndex);
    }
    
    [TestMethod]
    public async Task ShowAnswer_ReturnsConflict_WhenPinIsInvalid()
    {

        var request = new QuizAccessRequestDto
        {
            UserName = "Participant 2",
            Pin = "999999"
        };

        var response = await _client.PostAsJsonAsync(
            $"/quizzes/1/activeQuestion/answer",
            request);

        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode, content);
    }
    
    [TestMethod]
    public async Task SendMessage()
    {
        var request = new ChatRequestDto
        {
            Message = "hello",
            QuizAccess = new QuizAccessRequestDto
            {
                UserName = "Participant 1",
                Pin = "000000"
            }
        };

        var response = await _client.PostAsJsonAsync($"/quizzes/1/chat", request);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
    
    [TestMethod]
    public async Task SendMessage_ReturnsConflict_WhenPinIsInvalid()
    {
        var request = new ChatRequestDto
        {
            Message = "hello",
            QuizAccess = new QuizAccessRequestDto
            {
                UserName = "Participant 1",
                Pin = "999999"
            }
        };

        var response = await _client.PostAsJsonAsync($"/quizzes/1/chat", request);
        var content = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode,content);
    }
    
    [TestMethod]
    public async Task GetMyQuiz_ShouldReturnOk_WhenUserOwnsQuiz()
    {
        var loginResponse = await _client.PostAsJsonAsync("/users/login", _loginRequest);
        Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.IsNotNull(loginBody);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.AuthToken);
        
        var response = await _client.GetAsync($"/quizzes/my-quizzes/1");
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, content);

        var body = await response.Content.ReadFromJsonAsync<ExtendedQuizResponseDto>();

        Assert.IsNotNull(body);
        Assert.AreEqual("Test Quiz 1", body.Title);
    }
    
    [TestMethod]
    public async Task GetMyQuiz_ReturnsUnauthorized_WhenNoToken()
    {
        var response = await _client.GetAsync("/quizzes/my-quizzes/1");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    private async Task SeedDataBase(QuizDbContext context,UserManager<User> userManager)
    {
        userManager.CreateAsync(new User
        {
            UserName = "Test",
            Email = "test@test.com"
        }, "Test123*").Wait();
        var user = userManager.FindByEmailAsync("test@test.com").Result;
        if (user is null) return;
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
                User = user
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
            User = user
        });
    
        context.Quizzes.AddRange(quizzes);
        await context.SaveChangesAsync();
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        using var serviceScope = _server.Services.CreateScope();
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<QuizDbContext>();
        dbContext.Database.EnsureDeleted();
    }
}
