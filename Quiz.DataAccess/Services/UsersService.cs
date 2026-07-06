using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Quiz.DataAccess.Config;
using Quiz.DataAccess.Exceptions;
using Quiz.DataAccess.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Quiz.DataAccess.Services
{
    public class UsersService(
        QuizDbContext context,
        IOptions<JwtSettings> jwtSettings,
        IHttpContextAccessor httpContextAccessor,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IOptions<BigDataSettings> bigDataSettings)
        : IUsersService
    {

        private readonly JwtSettings _jwtSettings = jwtSettings.Value;
        private readonly BigDataSettings _bigDataSettings = bigDataSettings.Value;

        public async Task RegisterAsync(User user, string password)
        {
            var userExists = await userManager.FindByEmailAsync(user.Email!);
            if (userExists is not null) throw new InvalidDataException($"The email '{user.Email}' is already registered.");
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded) throw new InvalidDataException($"Registration failed: {result.Errors.First().Description}");
        }

        public async Task<(string authToken, string refreshToken, string userId)> LoginAsync(string email, string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) throw new AccessViolationException("There is no registered account with this email");

            var result = await signInManager.PasswordSignInAsync(user, password, false, false);
            if (!result.Succeeded) throw new AccessViolationException("Password is incorrect");

            var refreshToken = Guid.NewGuid();
            user.RefreshTokens.Add(refreshToken);
            await userManager.UpdateAsync(user);

            var authToken = GenerateJwtToken(user);

            return (authToken, refreshToken.ToString(), user.Id);
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return;
            if (!Guid.TryParse(refreshToken, out var guid)) throw new AccessViolationException("Invalid refresh token");
            user.RefreshTokens.Remove(guid);
            await userManager.UpdateAsync(user);

            await signInManager.SignOutAsync();
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirstValue("id");
            if (userId == null) return null;

            return await userManager.FindByIdAsync(userId);
        }

        public async Task<(string authToken, string refreshToken, string userId)> RedeemRefreshTokenAsync(string refreshToken)
        {
            if (!Guid.TryParse(refreshToken, out var parsedToken)) throw new AccessViolationException("Invalid refresh token");
            
            var user = await userManager.Users.FirstOrDefaultAsync(x => x.RefreshTokens.Contains(parsedToken));
            if (user is null) throw new AccessViolationException("Invalid refresh token");
            user.RefreshTokens.Remove(parsedToken);
            Guid newRefreshToken = Guid.NewGuid();
            user.RefreshTokens.Add(newRefreshToken);
            await userManager.UpdateAsync(user);
            
            var accessToken = GenerateJwtToken(user);
            return (accessToken, newRefreshToken.ToString(), user.Id);
        }

        public async Task<(User user, int totalCount, int pageSize)> GetUserAsync(string userId, int page)
        {
            page = Math.Max(page, 1);
            var pageSize = _bigDataSettings.PageSize;

            var currentUser = await GetCurrentUserAsync()
                ?? throw new InvalidOperationException("You have to login");

            if (currentUser.Id != userId)
                throw new InvalidOperationException("You do not have permission");

            var totalCount = await context.Quizzes
                .CountAsync(q => q.User.Id == userId);

            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null) throw new EntityNotFoundException(nameof(user));
                

            user.Quizzes = await context.Quizzes
                .AsNoTracking()
                .Where(q => q.User.Id == userId)
                .Include(q => q.Questions)
                .OrderBy(q => q.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (user, totalCount, pageSize);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("name",user.UserName!),
                new("id",user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
