using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Magic;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddUserSecrets<Program>();
    })
    .ConfigureServices((context, services) =>
    {
        // Configure Azure OpenAI settings
        services.Configure<AzureOpenAISettings>(
            context.Configuration.GetSection(AzureOpenAISettings.SectionName));
        
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });
        services.AddTransient<IApp, HelloWorld>();
        services.AddTransient<FibonacciApp>();
    })
    .Build();

// Allow user to choose which app to run
Console.WriteLine("Choose an application to run:");
Console.WriteLine("1. Hello World");
Console.WriteLine("2. Fibonacci Multi-Agent Workflow");
Console.Write("Enter your choice (1 or 2): ");

var choice = Console.ReadLine();

IApp app;
if (choice == "2")
{
    app = host.Services.GetRequiredService<FibonacciApp>();
}
else
{
    app = host.Services.GetRequiredService<IApp>();
}

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


