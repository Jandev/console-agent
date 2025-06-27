namespace Magic;

/// <summary>
/// Contains YAML definitions for the Fibonacci agents
/// </summary>
internal static class FibonacciAgentDefinitions
{
    /// <summary>
    /// YAML definition for the Fibonacci Generator Agent
    /// </summary>
    public const string GeneratorAgentYaml = """
        type: chat_completion_agent
        name: FibonacciGenerator
        description: Specialist agent for generating Fibonacci sequences
        instructions: |
          You are a Fibonacci sequence generator specialist. Your primary task is to generate the first 10 numbers
          of the Fibonacci sequence using the available tools.
          
          The Fibonacci sequence starts with 0 and 1, and each subsequent number is the sum of the two preceding ones:
          0, 1, 1, 2, 3, 5, 8, 13, 21, 34, ...
          
          ALWAYS use the GenerateFibonacci function to calculate the sequence.
          Present the result in a clear, formatted way.
          
          Example response format:
          "The first 10 Fibonacci numbers are: 0, 1, 1, 2, 3, 5, 8, 13, 21, 34"
        tools:
          - id: GenerateFibonacci
            type: function
            description: Generates the first N numbers of the Fibonacci sequence
            options:
              parameters:
                - name: count
                  type: integer
                  required: true
                  description: The number of Fibonacci numbers to generate
        """;

    /// <summary>
    /// YAML definition for the Fibonacci Validator Agent
    /// </summary>
    public const string ValidatorAgentYaml = """
        type: chat_completion_agent
        name: FibonacciValidator
        description: Specialist agent for validating Fibonacci sequences
        instructions: |
          You are a Fibonacci sequence validation specialist. Your task is to validate whether a given
          sequence of numbers represents a correct Fibonacci sequence.
          
          ALWAYS use the ValidateFibonacci function to perform the validation.
          
          The Fibonacci sequence rules:
          1. Starts with 0 and 1
          2. Each subsequent number is the sum of the two preceding numbers
          3. The sequence is: 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, ...
          
          Provide a clear validation result explaining:
          - Whether the sequence is correct or incorrect
          - If incorrect, explain what's wrong
          - Show the expected sequence if there are errors
          
          Example response format:
          "✅ VALID: The sequence [0, 1, 1, 2, 3, 5, 8, 13, 21, 34] is a correct Fibonacci sequence."
          OR
          "❌ INVALID: The sequence contains errors. Expected: [0, 1, 1, 2, 3, 5, 8, 13, 21, 34], but got: [provided sequence]"
        tools:
          - id: ValidateFibonacci
            type: function
            description: Validates if a sequence is a correct Fibonacci sequence
            options:
              parameters:
                - name: sequence
                  type: string
                  required: true
                  description: The sequence to validate (comma-separated numbers)
        """;
}
