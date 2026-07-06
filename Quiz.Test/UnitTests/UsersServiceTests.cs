using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Quiz.DataAccess;
using Quiz.DataAccess.Config;
using Quiz.DataAccess.Models;
using Quiz.DataAccess.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;


namespace Quiz.Test.UnitTests;

[TestClass]
public class UsersServiceTests : IDisposable
{
    private QuizDbContext _context = null!;
    private UsersService _usersService = null!;
    
    private Mock<UserManager<User>> _mockUserManager = null!;
    private Mock<SignInManager<User>> _mockSignInManager = null!;
    private HttpContextAccessor _httpContextAccessor = null!;

    [TestInitialize]
    public async Task Initialize()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase("TestUsersServiceDatabase")
            .Options;

        _context = new QuizDbContext(options);
        
        var jwtSettings = Options.Create(new JwtSettings
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 10,
            SecretKey = "TestSecretKeyTestSecretKeyTestSecretKey"
        });

        var bigDataSettings = Options.Create(new BigDataSettings { PageSize = 10 });

        var userStore = new Mock<IUserStore<User>>();

        _mockUserManager = new Mock<UserManager<User>>(
            userStore.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );

        var contextAccessorMock = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
        var identityOptions = Options.Create(new IdentityOptions());
        var logger = new Mock<ILogger<SignInManager<User>>>();
        var schemes = new Mock<IAuthenticationSchemeProvider>();
        var confirmation = new Mock<IUserConfirmation<User>>();

        _mockSignInManager = new Mock<SignInManager<User>>(
            _mockUserManager.Object,
            contextAccessorMock.Object,
            claimsFactory.Object,
            identityOptions,
            logger.Object,
            schemes.Object,
            confirmation.Object
        );

        _httpContextAccessor = new HttpContextAccessor();

        _usersService = new UsersService(
            _context,
            jwtSettings,
            _httpContextAccessor,
            _mockUserManager.Object,
            _mockSignInManager.Object,
            bigDataSettings
        );

        _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((string email) => _context.Users.FirstOrDefault(x => x.Email == email));
        
        _mockUserManager.Setup(x=>x.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync((User user,string password) =>
        {
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
            _context.Users.Add(user);
            _context.SaveChanges();
            return IdentityResult.Success;
        });

        _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User user) =>
        {
            _context.Users.Update(user);
            return IdentityResult.Success;
        });
        
        _mockUserManager
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => _context.Users.FirstOrDefault(x => x.Id == id));
        
        _mockUserManager
            .Setup(x => x.Users)
            .Returns(_context.Users);
        
        await SeedDataBase();
        
    }

    [TestMethod]
    public async Task RegisterAsync_ThrowsInvalidDataException_WhenEmailIsTaken()
    {
        await Assert.ThrowsExceptionAsync<InvalidDataException>(() => _usersService.RegisterAsync(new User{Email = "test@test.com",UserName = "Test"},"Test123*"));
    }
    
    [TestMethod]
    public async Task RegisterAsync()
    {
        await _usersService.RegisterAsync(new User { Email = "newtest@newtest.com", UserName = "NewTest" }, "Test123*");
        var user = await _context.Users.FirstOrDefaultAsync(x=>x.Email == "newtest@newtest.com");
        Assert.IsNotNull(user);
    }

    [TestMethod]
    public async Task LoginAsync_ThrowsAccessViolationException_WhenEmailNotFound()
    {
        await Assert.ThrowsExceptionAsync<AccessViolationException>(() => _usersService.LoginAsync("notfound@notfound.com", "Test123*"));
    }

    [TestMethod]
    public async Task LoginAsync_ThrowsAccessViolationException_WhenPasswordIsIncorrect()
    {
        _mockSignInManager  
            .Setup(x => x.PasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Failed);
        await Assert.ThrowsExceptionAsync<AccessViolationException>(() =>
            _usersService.LoginAsync("test@test.com", "Incorrect Password"));
    }

    [TestMethod]
    public async Task LoginAsync()
    {
        _mockSignInManager
            .Setup(x => x.PasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);
        var res = await _usersService.LoginAsync("test@test.com", "Test123*");
        Assert.IsFalse(res.authToken.IsNullOrEmpty());
        Assert.IsFalse(res.refreshToken.IsNullOrEmpty());
        Assert.AreEqual((await _context.Users.FirstAsync(x=>x.Email == "test@test.com")).Id, res.userId);
    }
    

    [TestMethod]
    public async Task GetCurrentUserAsync_ReturnsUser_WhenUserIsLoggedIn()
    {
        var user = await _usersService.GetCurrentUserAsync();
        Assert.IsNull(user);
        
        user = await _context.Users.FirstAsync();
        _httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("id", user.Id)
            ], "TestAuth"))
        };
        
        _mockSignInManager
            .Setup(x => x.PasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);
        var currentUser = await _usersService.GetCurrentUserAsync();
        Assert.AreSame(currentUser,user);
    }

    [TestMethod]
    public async Task LogoutAsync_ThrowsAccessViolationException_WhenRefreshTokenIsInvalid()
    {
        var user = await _context.Users.FirstAsync();
        _httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("id", user.Id)
            ], "TestAuth"))
        };

        await Assert.ThrowsExceptionAsync<AccessViolationException>(() => _usersService.LogoutAsync("Invalid RefreshToken"));
    }

    [TestMethod]
    public async Task LogoutAsync()
    {
        var user = await _context.Users.FirstAsync();
        _httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("id", user.Id)
            ], "TestAuth"))
        };
        
        _mockSignInManager
            .Setup(x => x.PasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);
        var res = await _usersService.LoginAsync("test@test.com", "Test123*");

        await _usersService.LogoutAsync(res.refreshToken);
        _mockSignInManager.Verify(x=>x.SignOutAsync(),Times.Once);
        Assert.AreEqual(0,(await _context.Users.FirstAsync(x=>x.Email == "test@test.com")).RefreshTokens.Count);
    }

    [TestMethod]
    public async Task RedeemRefreshTokenAsync_ThrowsAccessViolationException_WhenRefreshTokenIsInvalid()
    {
        await Assert.ThrowsExceptionAsync<AccessViolationException>(() => _usersService.RedeemRefreshTokenAsync("Invalid RefreshToken"));
    }

    [TestMethod]
    public async Task RedeemRefreshTokenAsync_ThrowsAccessViolationException_WhenRefreshTokenIsNotFound()
    {
        await Assert.ThrowsExceptionAsync<AccessViolationException>(() =>
            _usersService.RedeemRefreshTokenAsync(Guid.NewGuid().ToString()));
    }

    [TestMethod]
    public async Task RedeemRefreshTokenAsync()
    {
        var user = await _context.Users.FirstAsync();
        var oldRefreshToken = Guid.NewGuid();

        user.RefreshTokens.Add(oldRefreshToken);
        await _context.SaveChangesAsync();

        var result = await _usersService.RedeemRefreshTokenAsync(oldRefreshToken.ToString());

        Assert.IsFalse(result.authToken.IsNullOrEmpty());
        Assert.IsFalse(result.refreshToken.IsNullOrEmpty());
        Assert.AreEqual(user.Id, result.userId);

        Assert.IsFalse(user.RefreshTokens.Contains(oldRefreshToken));
        Assert.AreEqual(1, user.RefreshTokens.Count);

        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [TestMethod]
    public async Task GetUserAsync_ThrowsInvalidOperationException_WhenUserIsNotLoggedIn()
    {
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            _usersService.GetUserAsync(_context.Users.First().Id, 10));
    }
    
    [TestMethod]
    public async Task GetUserAsync_ThrowsInvalidOperationException_WhenUserIsNotMatching()
    {
        var user = await _context.Users.FirstAsync();
        _httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("id", user.Id)
            ], "TestAuth"))
        };
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            _usersService.GetUserAsync("Not user's id", 10));
    }
    
    [TestMethod]
    public async Task GetUserAsync()
    {
        var user = await _context.Users.FirstAsync();
        _httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("id", user.Id)
            ], "TestAuth"))
        };

        List<DataAccess.Models.Quiz> quizzes = new List<DataAccess.Models.Quiz>();
        for(int i = 1; i < 12; i++)
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
        _context.Quizzes.AddRange(quizzes);
        await _context.SaveChangesAsync();
        
        var res = await _usersService.GetUserAsync(user.Id, 1);
        Assert.AreEqual(11,res.totalCount);
        Assert.AreEqual(10,res.user.Quizzes.Count);
        res = await _usersService.GetUserAsync(user.Id, 2);
        Assert.AreEqual(11,res.totalCount);
        Assert.AreEqual(1,res.user.Quizzes.Count);
    }
    
    private async Task SeedDataBase()
    {
        var user = new User
        {
            UserName = "Test",
            Email = "test@test.com",
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "Test123*");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
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