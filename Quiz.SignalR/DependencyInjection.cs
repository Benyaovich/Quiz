using Microsoft.Extensions.DependencyInjection;
using Quiz.SignalR.Services;

namespace Quiz.SignalR
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSignalRServices(this IServiceCollection services)
        {
            services.AddSingleton<IQuizNotificationService, QuizNotificationService>();

            return services;
        }
    }
}
