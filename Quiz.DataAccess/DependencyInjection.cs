using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quiz.DataAccess.Models;
using Quiz.DataAccess.Services;

namespace Quiz.DataAccess
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("DefaultConnection");
            services.AddDbContext<QuizDbContext>(options => options.UseSqlServer(connectionString));
            services.AddIdentity<User, UserRole>().AddEntityFrameworkStores<QuizDbContext>().AddDefaultTokenProviders();

            services.AddScoped<IQuizzesService, QuizzesService>();
            services.AddScoped<IUsersService, UsersService>();

            return services;
        }
    }
}
