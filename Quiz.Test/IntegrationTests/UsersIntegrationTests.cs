using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Quiz.DataAccess;
using Quiz.Shared.Requests;
using Quiz.Shared.Responses;

namespace Quiz.Test.IntegrationTests;

[TestClass]
public class UsersIntegrationTest
{
    private WebApplicationFactory<Program> _server = null!;
    private HttpClient _client = null!;
    
    private readonly RegisterRequestDto _registerRequest = new()
    {
        Email = "test@test.com",
        UserName = "test",
        Password = "Test123*"
    };
    
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
                services.AddDbContext<QuizDbContext>(options => options.UseInMemoryDatabase("UsersIntegrationTests"));
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<QuizDbContext>();
                dbContext.Database.EnsureCreated();
            });
        });
        
        _client = _server.CreateClient();
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        

    }

    [TestMethod]
    public async Task Register()
    {
        
        var response = await _client.PostAsJsonAsync("/users/register", _registerRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, content);
        
        var body = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();

        Assert.IsNotNull(body);
        Assert.AreEqual(_registerRequest.Email, body.Email);
        Assert.AreEqual(_registerRequest.UserName, body.UserName);
    }
    
    [TestMethod]
    public async Task Register_ReturnsConflict_WhenUserAlreadyExists()
    {
        var firstResponse = await _client.PostAsJsonAsync("/users/register", _registerRequest);
        Assert.AreEqual(HttpStatusCode.Created, firstResponse.StatusCode);

        var secondResponse = await _client.PostAsJsonAsync("/users/register", _registerRequest);
        Assert.AreEqual(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }
    
    [TestMethod]
    public async Task Register_ShouldReturnBadRequest_WhenDtoIsInvalid()
    {
        var request = new RegisterRequestDto
        {
            Email = "invalid email",
            UserName = "",
            Password = ""
        };

        var response = await _client.PostAsJsonAsync("/users/register", request);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Login()
    {
        var response = await _client.PostAsJsonAsync("/users/register", _registerRequest);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        response = await _client.PostAsJsonAsync("/users/login", _loginRequest);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

        Assert.IsNotNull(body);
        Assert.IsFalse(body.AuthToken.IsNullOrEmpty());
        Assert.IsFalse(body.RefreshToken.IsNullOrEmpty());
        Assert.IsFalse(body.UserId.IsNullOrEmpty());
    }
    
    [TestMethod]
    public async Task Login_ReturnsForbidden_WhenEmailIsNotExist()
    {
        var response = await _client.PostAsJsonAsync("/users/login", _loginRequest);
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [TestMethod]
    public async Task Logout_ReturnsUnauthorized_WhenNoToken()
    {
        var response = await _client.PostAsJsonAsync("/users/logout", "");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task Logout()
    {
        var response = await _client.PostAsJsonAsync("/users/register", _registerRequest);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        
        response = await _client.PostAsJsonAsync("/users/login", _loginRequest);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.IsNotNull(body);
        
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body.AuthToken);
        
        response = await _client.PostAsJsonAsync("/users/logout", body.RefreshToken);
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [TestMethod]
    public async Task RedeemRefreshToken()
    {
        var response = await _client.PostAsJsonAsync("/users/register", _registerRequest);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        response = await _client.PostAsJsonAsync("/users/login", _loginRequest);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.IsNotNull(body);
        
        response = await _client.PostAsJsonAsync("/users/refresh", body.RefreshToken);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.IsNotNull(body);
        Assert.IsFalse(body.AuthToken.IsNullOrEmpty());
        Assert.IsFalse(body.RefreshToken.IsNullOrEmpty());
        Assert.IsFalse(body.UserId.IsNullOrEmpty());
        
    }
    
    [TestMethod]
    public async Task RedeemRefreshToken_ReturnsForbidden_WhenRefreshTokenIsInvalid()
    {
        var response = await _client.PostAsJsonAsync("/users/refresh", "invalidRefreshToken");
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [TestMethod]
    public async Task GetUser()
    {
        var response = await _client.PostAsJsonAsync("/users/register", _registerRequest);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        response = await _client.PostAsJsonAsync("/users/login", _loginRequest);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var loginBody = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.IsNotNull(loginBody);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.AuthToken);

        response = await _client.GetAsync($"/users/{loginBody.UserId}?page=1");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedUserResponseDto>();
        Assert.IsNotNull(body);
        
        Assert.AreEqual(body.User.Email, _loginRequest.Email);
        Assert.AreEqual(0,body.User.Quizzes.Count);
    }
    
    [TestMethod]
    public async Task GetUser_ShouldReturnUnauthorized_WhenNoToken()
    {
        var response = await _client.GetAsync("/users/1?page=1");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestCleanup]
    public void Cleanup()
    {
        using var serviceScope = _server.Services.CreateScope();
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<QuizDbContext>();
        dbContext.Database.EnsureDeleted();
    }
}