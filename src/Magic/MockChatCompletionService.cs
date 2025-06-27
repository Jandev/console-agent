using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Runtime.CompilerServices;

namespace Magic;

/// <summary>
/// Mock chat completion service for demonstration purposes
/// In production, you would use actual OpenAI or Azure OpenAI service
/// </summary>
internal class MockChatCompletionService : IChatCompletionService
{
    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory, 
        PromptExecutionSettings? executionSettings = null, 
        Kernel? kernel = null, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await GetChatMessageContentAsync(chatHistory, executionSettings, kernel, cancellationToken);
        yield return new StreamingChatMessageContent(response.Role, response.Content);
    }

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory, 
        PromptExecutionSettings? executionSettings = null, 
        Kernel? kernel = null, 
        CancellationToken cancellationToken = default)
    {
        var response = await GetChatMessageContentAsync(chatHistory, executionSettings, kernel, cancellationToken);
        return new[] { response };
    }

    private async Task<ChatMessageContent> GetChatMessageContentAsync(
        ChatHistory chatHistory, 
        PromptExecutionSettings? executionSettings = null, 
        Kernel? kernel = null, 
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken); // Simulate processing delay

        var lastMessage = chatHistory.LastOrDefault();
        if (lastMessage == null)
        {
            return new ChatMessageContent(AuthorRole.Assistant, "I'm ready to help with Fibonacci sequences!");
        }

        var userMessage = lastMessage.Content?.ToLowerInvariant() ?? "";
        
        // Simple pattern matching for Fibonacci-related responses
        if (userMessage.Contains("generate") && userMessage.Contains("fibonacci"))
        {
            // Simulate calling the plugin
            if (kernel != null)
            {
                try
                {
                    var plugin = kernel.Plugins.FirstOrDefault(p => p.Name.Contains("Fibonacci"));
                    if (plugin != null)
                    {
                        var generateFunction = plugin.FirstOrDefault(f => f.Name == "GenerateFibonacci");
                        if (generateFunction != null)
                        {
                            var result = await generateFunction.InvokeAsync(kernel, new KernelArguments { ["count"] = 10 });
                            var sequence = result.GetValue<int[]>();
                            if (sequence != null)
                            {
                                return new ChatMessageContent(AuthorRole.Assistant, 
                                    $"The first 10 Fibonacci numbers are: {string.Join(", ", sequence)}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new ChatMessageContent(AuthorRole.Assistant, 
                        $"Error generating Fibonacci sequence: {ex.Message}");
                }
            }
            
            return new ChatMessageContent(AuthorRole.Assistant, 
                "The first 10 Fibonacci numbers are: 0, 1, 1, 2, 3, 5, 8, 13, 21, 34");
        }
        
        if (userMessage.Contains("validate") && userMessage.Contains("fibonacci"))
        {
            // Extract sequence from the message
            var sequencePart = ExtractSequenceFromMessage(userMessage);
            
            if (kernel != null)
            {
                try
                {
                    var plugin = kernel.Plugins.FirstOrDefault(p => p.Name.Contains("Fibonacci"));
                    if (plugin != null)
                    {
                        var validateFunction = plugin.FirstOrDefault(f => f.Name == "ValidateFibonacci");
                        if (validateFunction != null)
                        {
                            var result = await validateFunction.InvokeAsync(kernel, new KernelArguments { ["sequence"] = sequencePart });
                            var validationResult = result.GetValue<string>();
                            if (!string.IsNullOrEmpty(validationResult))
                            {
                                return new ChatMessageContent(AuthorRole.Assistant, validationResult);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new ChatMessageContent(AuthorRole.Assistant, 
                        $"Error validating Fibonacci sequence: {ex.Message}");
                }
            }
            
            return new ChatMessageContent(AuthorRole.Assistant, 
                "I'll validate the sequence for you. Please provide the sequence to validate.");
        }

        return new ChatMessageContent(AuthorRole.Assistant, 
            "I'm a Fibonacci specialist. I can generate or validate Fibonacci sequences. How can I help you?");
    }

    private static string ExtractSequenceFromMessage(string message)
    {
        // Look for sequences like "0, 1, 1, 2, 3, 5" in the message
        var parts = message.Split(':');
        for (int i = 1; i < parts.Length; i++)
        {
            var part = parts[i].Trim();
            if (part.Contains(',') || char.IsDigit(part.FirstOrDefault()))
            {
                // Extract until next sentence or end
                var sequenceEnd = part.IndexOfAny(new[] { '.', '?', '!', '\n' });
                if (sequenceEnd > 0)
                {
                    part = part.Substring(0, sequenceEnd);
                }
                return part.Trim();
            }
        }
        
        // Default sequence for validation
        return "0, 1, 1, 2, 3, 5, 8, 13, 21, 34";
    }
}
