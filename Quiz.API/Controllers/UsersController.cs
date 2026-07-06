using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quiz.DataAccess.Models;
using Quiz.DataAccess.Services;
using Quiz.Shared.Requests;
using Quiz.Shared.Responses;

namespace Quiz.API.Controllers
{

    [ApiController]
    [Route("/users")]
    public class UsersController(IUsersService userService, IMapper mapper) : ControllerBase
    {
        [Route("register")]
        [HttpPost]
        [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto userRequestDto)
        {
            var user = mapper.Map<User>(userRequestDto);
            await userService.RegisterAsync(user, userRequestDto.Password);
            var registerResponseDto = mapper.Map<RegisterResponseDto>(user);
            return StatusCode(StatusCodes.Status201Created, registerResponseDto);
        }

        [Route("login")]
        [HttpPost]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            var (authToken, refreshToken, userId) = await userService.LoginAsync(loginRequestDto.Email, loginRequestDto.Password);

            var loginResponseDto = new LoginResponseDto
            {
                UserId = userId,
                AuthToken = authToken,
                RefreshToken = refreshToken,
            };

            return Ok(loginResponseDto);
        }

        [Route("logout")]
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            await userService.LogoutAsync(refreshToken);

            return NoContent();
        }

        [HttpPost]
        [Route("refresh")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RedeemRefreshToken([FromBody] string refreshToken)
        {
            var (authToken, newRefreshToken, userId) = await userService.RedeemRefreshTokenAsync(refreshToken);

            var loginResponseDto = new LoginResponseDto
            {
                UserId = userId,
                AuthToken = authToken,
                RefreshToken = newRefreshToken,
            };

            return Ok(loginResponseDto);
        }

        [HttpGet]
        [Route("{userId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(PagedUserResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUser([FromRoute] string userId, [FromQuery] int page)
        {
            var (user,totalCount,pageSize) = await userService.GetUserAsync(userId, page);
            var userResponseDto = mapper.Map<UserResponseDto>(user);

            return Ok(
                new PagedUserResponseDto
                {
                    User = userResponseDto, Pagination = new PaginationResponseDto
                    {
                        TotalCount = totalCount,
                        PageSize = pageSize
                    }
                }
            );
        }
    }
}
