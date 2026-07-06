using AutoMapper;

namespace Quiz.API.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAutomapper(this IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(config => config.AddProfile(new MappingProfile()));
            mapperConfig.AssertConfigurationIsValid();

            services.AddAutoMapper(typeof(MappingProfile));

            return services;
        }
    }
}
