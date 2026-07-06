using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Quiz.DataAccess;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        config.AddUserSecrets<Program>();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDataAccess(hostContext.Configuration);
    }).Build();

await host.StartAsync();
