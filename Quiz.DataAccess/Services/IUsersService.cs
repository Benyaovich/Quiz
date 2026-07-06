using Quiz.DataAccess.Models;

namespace Quiz.DataAccess.Services
{
    public interface IUsersService
    {
        Task RegisterAsync(User user, string password);
        Task<(string authToken, string refreshToken, string userId)> LoginAsync(string email, string password);
        Task LogoutAsync(string refreshToken);
        Task<(string authToken, string refreshToken, string userId)> RedeemRefreshTokenAsync(string refreshToken);
        Task<User?> GetCurrentUserAsync();
        Task<(User user, int totalCount, int pageSize)> GetUserAsync(string userId, int page);
    }
}
