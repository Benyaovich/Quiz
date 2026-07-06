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

namespace Quiz.Test.ControllerTests;

[TestClass]
public class UsersControllerTests
{
    private UsersController _usersController = null!;
    private Mock<IUsersService> _mockUsersService = null!;

    [TestInitialize]
    public void Initialize()
    {
        var mappingConfig = new MapperConfiguration(config => config.AddProfile(new MappingProfile()));
        var mapper = mappingConfig.CreateMapper();
        
        _mockUsersService = new Mock<IUsersService>();
        _usersController = new UsersController(_mockUsersService.Object, mapper);
    }

    [TestMethod]
    public async Task Register()
    {
        var request = new RegisterRequestDto
        {
            UserName = "Test User",
            Email = "test@test.com",
            Password = "Test123*"
        };

        _mockUsersService
            .Setup(x => x.RegisterAsync(It.IsAny<User>(), request.Password))
            .Returns(Task.CompletedTask);

        var result = await _usersController.Register(request);
        
        _mockUsersService.Verify(x =>
                x.RegisterAsync(
                    It.Is<User>(u =>
                        u.UserName == request.UserName &&
                        u.Email == request.Email),
                    request.Password),
            Times.Once);
        
        Assert.IsInstanceOfType<ObjectResult>(result);
        var obj = result as ObjectResult;
        Assert.IsNotNull(obj);
        Assert.AreEqual(StatusCodes.Status201Created, obj.StatusCode);
        
        Assert.IsInstanceOfType<RegisterResponseDto>(obj.Value);
        var response = obj.Value as RegisterResponseDto;
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task Login()
    {
        var request = new LoginRequestDto
        {
            Email = "test@test.com",
            Password = "Test123*"
        };

        _mockUsersService
            .Setup(x => x.LoginAsync(request.Email, request.Password))
            .ReturnsAsync(("authToken", "refreshToken", "userId"));
        
        var result = await _usersController.Login(request);
        
        _mockUsersService.Verify(x =>
                x.LoginAsync(request.Email, request.Password),
            Times.Once);
        
        Assert.IsInstanceOfType<OkObjectResult>(result);
        var obj = result as OkObjectResult;
        Assert.IsNotNull(obj);
        Assert.AreEqual(StatusCodes.Status200OK, obj.StatusCode);

        Assert.IsInstanceOfType<LoginResponseDto>(obj.Value);
        var response = obj.Value as LoginResponseDto;
        Assert.IsNotNull(response);
    }
    
    [TestMethod]
    public async Task Logout()
    {
        var refreshToken = "refreshToken";

        _mockUsersService
            .Setup(x => x.LogoutAsync(refreshToken))
            .Returns(Task.CompletedTask);
        
        var result = await _usersController.Logout(refreshToken);
        _mockUsersService.Verify(x =>
                x.LogoutAsync(refreshToken),
            Times.Once);
        
        Assert.IsInstanceOfType<NoContentResult>(result);
        var obj = result as NoContentResult;
        Assert.IsNotNull(obj);
        Assert.AreEqual(StatusCodes.Status204NoContent, obj.StatusCode);
    }
    
    [TestMethod]
    public async Task RedeemRefreshToken()
    {
        var refreshToken = "oldRefreshToken";

        _mockUsersService
            .Setup(x => x.RedeemRefreshTokenAsync(refreshToken))
            .ReturnsAsync(("newAuthToken", "newRefreshToken", "userId"));
        
        var result = await _usersController.RedeemRefreshToken(refreshToken);

        _mockUsersService.Verify(x =>
                x.RedeemRefreshTokenAsync(refreshToken),
            Times.Once);
        
        Assert.IsInstanceOfType<OkObjectResult>(result);
        var obj = result as OkObjectResult;
        Assert.IsNotNull(obj);
        Assert.AreEqual(StatusCodes.Status200OK, obj.StatusCode);

        Assert.IsInstanceOfType<LoginResponseDto>(obj.Value);
        var response = obj.Value as LoginResponseDto;
        Assert.IsNotNull(response);
    }
    
    [TestMethod]
    public async Task GetUser()
    {
        var user = new User
        {
            Id = "userId",
            UserName = "Test User",
            Email = "test@test.com"
        };

        _mockUsersService
            .Setup(x => x.GetUserAsync("userId", 1))
            .ReturnsAsync((user, 25, 10));
        
        var result = await _usersController.GetUser("userId", 1);
        
        _mockUsersService.Verify(x =>
                x.GetUserAsync("userId", 1),
            Times.Once);
        
        Assert.IsInstanceOfType<OkObjectResult>(result);
        var obj = result as OkObjectResult;
        Assert.IsNotNull(obj);
        Assert.AreEqual(StatusCodes.Status200OK, obj.StatusCode);

        Assert.IsInstanceOfType<PagedUserResponseDto>(obj.Value);
        var response = obj.Value as PagedUserResponseDto;
        Assert.IsNotNull(response);
    }
}