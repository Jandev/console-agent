using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Magic;

/// <summary>
/// Fibonacci app that uses multi-agent workflow to generate and validate Fibonacci sequence
/// </summary>
internal class FibonacciApp : IApp
{
    private readonly ILogger<FibonacciApp> _logger;

    public FibonacciApp(ILogger<FibonacciApp> logger)
    {
        _logger = logger;
    }

    public void Run()
    {
        _logger.LogInformation("Starting Fibonacci Multi-Agent Workflow...");
        RunAsync().GetAwaiter().GetResult();
    }

    private async Task RunAsync()
    {
        try
        {
            // Create kernel with Fibonacci plugin
            var kernel = CreateKernel();

            // Create specialized agents
            var generatorAgent = CreateFibonacciGeneratorAgent(kernel);
            var validatorAgent = CreateFibonacciValidatorAgent(kernel);

            _logger.LogInformation("Agents created successfully.");
            _logger.LogInformation("Generator Agent: {GeneratorName}", generatorAgent.Name);
            _logger.LogInformation("Validator Agent: {ValidatorName}", validatorAgent.Name);

            // Start interactive session
            await RunInteractiveSessionAsync(generatorAgent, validatorAgent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Fibonacci workflow");
        }
    }

    private async Task RunInteractiveSessionAsync(ChatCompletionAgent generatorAgent, ChatCompletionAgent validatorAgent)
    {
        Console.WriteLine("\n=== Fibonacci Multi-Agent Workflow ===");
        Console.WriteLine("Available commands:");
        Console.WriteLine("- 'generate' - Generate first 10 Fibonacci numbers");
        Console.WriteLine("- 'validate' - Validate the generated sequence");
        Console.WriteLine("- 'both' - Generate and validate");
        Console.WriteLine("- 'exit' - Exit the application");

        string? userInput;
        string? fibonacciSequence = null;

        do
        {
            Console.Write("\nWhat would you like to do? ");
            userInput = Console.ReadLine()?.Trim().ToLowerInvariant();

            switch (userInput)
            {
                case "generate":
                    fibonacciSequence = await GenerateFibonacciAsync(generatorAgent);
                    break;

                case "validate":
                    if (!string.IsNullOrEmpty(fibonacciSequence))
                    {
                        await ValidateFibonacciAsync(validatorAgent, fibonacciSequence);
                    }
                    else
                    {
                        Console.WriteLine("No sequence to validate. Generate a sequence first.");
                    }
                    break;

                case "both":
                    fibonacciSequence = await GenerateFibonacciAsync(generatorAgent);
                    if (!string.IsNullOrEmpty(fibonacciSequence))
                    {
                        await ValidateFibonacciAsync(validatorAgent, fibonacciSequence);
                    }
                    break;

                case "exit":
                    Console.WriteLine("Goodbye!");
                    break;

                default:
                    Console.WriteLine("Invalid command. Please try again.");
                    break;
            }

        } while (userInput != "exit");
    }

    private async Task<string?> GenerateFibonacciAsync(ChatCompletionAgent generatorAgent)
    {
        Console.WriteLine("\n🔢 Generating Fibonacci sequence...");

        var request = "Generate the first 10 numbers of the Fibonacci sequence using the GenerateFibonacci function.";
        string? sequence = null;

        await foreach (var response in generatorAgent.InvokeAsync(request))
        {
            var content = response.Message?.Content ?? "";
            Console.WriteLine($"Generator: {content}");
            sequence = content;
        }

        return sequence;
    }

    private async Task ValidateFibonacciAsync(ChatCompletionAgent validatorAgent, string sequence)
    {
        Console.WriteLine("\n✅ Validating Fibonacci sequence...");

        var validationRequest = $"Validate if this is a correct Fibonacci sequence using the ValidateFibonacci function: {sequence}";

        await foreach (var response in validatorAgent.InvokeAsync(validationRequest))
        {
            var content = response.Message?.Content ?? "";
            Console.WriteLine($"Validator: {content}");
        }
    }

    private ChatCompletionAgent CreateFibonacciGeneratorAgent(Kernel kernel)
    {
        return new ChatCompletionAgent
        {
            Name = "FibonacciGenerator",
            Instructions = FibonacciAgentInstructions.GeneratorInstructions,
            Kernel = kernel,
            Arguments = new KernelArguments()
        };
    }

    private ChatCompletionAgent CreateFibonacciValidatorAgent(Kernel kernel)
    {
        return new ChatCompletionAgent
        {
            Name = "FibonacciValidator", 
            Instructions = FibonacciAgentInstructions.ValidatorInstructions,
            Kernel = kernel,
            Arguments = new KernelArguments()
        };
    }

    private Kernel CreateKernel()
    {
        var builder = Kernel.CreateBuilder();

        // Add chat completion service (using our mock for demo)
        builder.Services.AddSingleton<IChatCompletionService, MockChatCompletionService>();

        // Add plugins for Fibonacci operations
        var fibonacciPlugin = KernelPluginFactory.CreateFromType<FibonacciPlugin>();
        builder.Plugins.Add(fibonacciPlugin);

        return builder.Build();
    }
}
