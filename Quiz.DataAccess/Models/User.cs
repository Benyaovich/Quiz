using Microsoft.AspNetCore.Identity;

namespace Quiz.DataAccess.Models
{
    public class User : IdentityUser
    {

        public List<Guid> RefreshTokens { get; set; } = new();

        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }
}
