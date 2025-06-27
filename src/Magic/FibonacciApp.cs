using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates.

namespace Magic;

/// <summary>
/// Fibonacci app that uses multi-agent workflow to generate and validate Fibonacci sequence
/// </summary>
internal class FibonacciApp : IApp
{
    private readonly ILogger<FibonacciApp> _logger;
    private readonly AzureOpenAISettings _azureOpenAISettings;

    public FibonacciApp(ILogger<FibonacciApp> logger, IOptions<AzureOpenAISettings> azureOpenAISettings)
    {
        _logger = logger;
        _azureOpenAISettings = azureOpenAISettings.Value;
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
        Console.WriteLine("\n=== Fibonacci Multi-Agent Assistant ===");
        Console.WriteLine("Ask me anything about Fibonacci numbers! I can:");
        Console.WriteLine("• Generate Fibonacci sequences (e.g., 'What are the first 8 Fibonacci numbers?')");
        Console.WriteLine("• Validate sequences (e.g., 'Is this sequence correct: 0, 1, 1, 2, 3, 5?')");
        Console.WriteLine("• Check individual numbers (e.g., 'Is 89 a Fibonacci number?')");
        Console.WriteLine("• Explain the Fibonacci sequence");
        Console.WriteLine("• Answer general questions (I'll use my knowledge to help with any topic!)");
        Console.WriteLine("\nType 'exit' to quit.\n");

        // Create a general assistant agent for non-Fibonacci questions
        var generalAgent = CreateGeneralAssistantAgent(CreateKernel());

        // Create an AgentGroupChat with all agents
        var agentGroupChat = new AgentGroupChat(generatorAgent, validatorAgent, generalAgent)
        {
            ExecutionSettings = new()
            {
                // Terminate after each agent responds once to give a complete answer
                TerminationStrategy = new FibonacciTerminationStrategy()
                {
                    MaximumIterations = 10, // Allow up to 10 interactions between agents
                    AutomaticReset = true // Reset after each user question
                }
            }
        };

        string? userInput;

        do
        {
            Console.Write("You: ");
            userInput = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                if (userInput?.Equals("exit", StringComparison.OrdinalIgnoreCase) == true)
                {
                    Console.WriteLine("\nGoodbye! Thanks for exploring Fibonacci numbers with me! 🔢");
                }
                continue;
            }

            await ProcessUserQuestionAsync(agentGroupChat, userInput);

        } while (userInput != null && !userInput.Equals("exit", StringComparison.OrdinalIgnoreCase));
    }

    private async Task ProcessUserQuestionAsync(AgentGroupChat agentGroupChat, string question)
    {
        try
        {
            // Add the user message to the chat
            agentGroupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, question));

            Console.WriteLine();

            // Let the agents collaborate to answer the question
            await foreach (var response in agentGroupChat.InvokeAsync())
            {
                var content = response.Content ?? "";
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
                var agentName = response.AuthorName ?? "Agent";
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
                
                // Add emoji based on agent name for better UX
                var emoji = agentName.Contains("Generator") ? "🔢" : 
                           agentName.Contains("Validator") ? "✅" : 
                           agentName.Contains("GeneralAssistant") ? "🤖" : "💬";
                
                if (!string.IsNullOrEmpty(content))
                {
                    Console.WriteLine($"{agentName} {emoji}: {content}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sorry, I encountered an error: {ex.Message}");
            _logger.LogError(ex, "Error processing user question: {Question}", question);
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

    private ChatCompletionAgent CreateGeneralAssistantAgent(Kernel kernel)
    {
        return new ChatCompletionAgent
        {
            Name = "GeneralAssistant",
            Instructions = FibonacciAgentInstructions.GeneralAssistantInstructions,
            Kernel = kernel,
            Arguments = new KernelArguments()
        };
    }

    private Kernel CreateKernel()
    {
        // Validate configuration
        if (string.IsNullOrEmpty(_azureOpenAISettings.Endpoint))
        {
            throw new InvalidOperationException("Azure OpenAI Endpoint is not configured. Please check your appsettings.json file.");
        }
        
        if (string.IsNullOrEmpty(_azureOpenAISettings.DeploymentName))
        {
            throw new InvalidOperationException("Azure OpenAI DeploymentName is not configured. Please check your appsettings.json file.");
        }
        
        if (string.IsNullOrEmpty(_azureOpenAISettings.ApiKey))
        {
            throw new InvalidOperationException("Azure OpenAI ApiKey is not configured. Please run: dotnet user-secrets set \"AzureOpenAI:ApiKey\" \"your-api-key-here\"");
        }

        var builder = Kernel.CreateBuilder();

        // Add Azure OpenAI chat completion service
        builder.Services.AddAzureOpenAIChatCompletion(
            deploymentName: _azureOpenAISettings.DeploymentName,
            endpoint: _azureOpenAISettings.Endpoint,
            apiKey: _azureOpenAISettings.ApiKey);

        // Add plugins for Fibonacci operations
        var fibonacciPlugin = KernelPluginFactory.CreateFromType<FibonacciPlugin>();
        builder.Plugins.Add(fibonacciPlugin);

        return builder.Build();
    }
}
