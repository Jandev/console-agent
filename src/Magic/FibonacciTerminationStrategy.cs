using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates.

namespace Magic;

/// <summary>
/// Custom termination strategy for Fibonacci multi-agent chat.
/// Terminates when both agents have provided responses or when a satisfactory answer is given.
/// </summary>
internal class FibonacciTerminationStrategy : TerminationStrategy
{
    /// <summary>
    /// Determines if the agent chat should terminate based on the conversation history.
    /// </summary>
    /// <param name="agent">The agent that just responded</param>
    /// <param name="history">The conversation history</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the chat should terminate, false otherwise</returns>
    protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken = default)
    {
        // If there's no history, don't terminate
        if (history.Count == 0)
        {
            return Task.FromResult(false);
        }

        // Get messages from agents (exclude user messages)
        var agentMessages = history.Where(h => h.Role == AuthorRole.Assistant).ToList();

        // If no agent has responded yet, don't terminate
        if (agentMessages.Count == 0)
        {
            return Task.FromResult(false);
        }

        // Get the unique agent names that have responded
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
        var respondingAgents = agentMessages
            .Select(m => GetAgentNameFromAuthor(m.AuthorName))
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .ToList();
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates.

        // Terminate if both agents have responded at least once
        bool bothAgentsResponded = respondingAgents.Count >= 2;

        // Also check if the last response contains indicators of completion
        var lastMessage = history.LastOrDefault();
        if (lastMessage?.Role == AuthorRole.Assistant)
        {
            var content = lastMessage.Content?.ToLowerInvariant() ?? "";
            
            // Look for completion indicators in the response
            var completionIndicators = new[]
            {
                "✅ valid", "❌ invalid", "correct fibonacci sequence", 
                "incorrect sequence", "here are the", "the fibonacci numbers are",
                "approved", "confirmed", "complete"
            };

            bool hasCompletionIndicator = completionIndicators.Any(indicator => content.Contains(indicator));

            // Terminate if we have a clear completion indicator and at least one agent responded
            if (hasCompletionIndicator && agentMessages.Count >= 1)
            {
                return Task.FromResult(true);
            }
        }

        // Terminate if both agents have responded
        return Task.FromResult(bothAgentsResponded);
    }

    /// <summary>
    /// Extracts the agent name from the author name, handling potential prefixes or formatting.
    /// </summary>
    /// <param name="authorName">The author name from the message</param>
    /// <returns>The clean agent name</returns>
    private static string? GetAgentNameFromAuthor(string? authorName)
    {
        if (string.IsNullOrEmpty(authorName))
        {
            return null;
        }

        // Handle agent names that might have prefixes or be formatted differently
        if (authorName.Contains("Generator") || authorName.Contains("FibonacciGenerator"))
        {
            return "Generator";
        }

        if (authorName.Contains("Validator") || authorName.Contains("FibonacciValidator"))
        {
            return "Validator";
        }

        return authorName;
    }
}
