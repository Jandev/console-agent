using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });
        services.AddTransient<IApp, HelloWorld>();
    })
    .Build();

var app = host.Services.GetRequiredService<IApp>();
app.Run();

await host.RunAsync();

public interface IApp
{
    void Run();
}

internal class HelloWorld : IApp
{
    private readonly ILogger<HelloWorld> _logger;

    public HelloWorld(ILogger<HelloWorld> logger)
    {
        _logger = logger;
    }

    public void Run()
    {
        _logger.LogInformation("Hello, World!");
    }
}


