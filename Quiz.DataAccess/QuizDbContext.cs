using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Quiz.DataAccess.Models;

namespace Quiz.DataAccess
{

    public class QuizDbContext(DbContextOptions<QuizDbContext> options)
        : IdentityDbContext<User, UserRole, string>(options)
    {
        public DbSet<Question> Questions { get; set; } = null!;
        public DbSet<Models.Quiz> Quizzes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }

    }

}
