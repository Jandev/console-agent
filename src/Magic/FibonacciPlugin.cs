using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Magic;

/// <summary>
/// Kernel plugin containing Fibonacci-related functions for agents to use
/// </summary>
public class FibonacciPlugin
{
    /// <summary>
    /// Generates the first N numbers of the Fibonacci sequence
    /// </summary>
    /// <param name="count">Number of Fibonacci numbers to generate</param>
    /// <returns>Array of Fibonacci numbers</returns>
    [KernelFunction]
    [Description("Generates the first N numbers of the Fibonacci sequence")]
    public int[] GenerateFibonacci(
        [Description("The number of Fibonacci numbers to generate")] int count)
    {
        if (count <= 0)
            return Array.Empty<int>();

        var sequence = new int[count];
        
        if (count >= 1)
            sequence[0] = 0;
        if (count >= 2)
            sequence[1] = 1;

        for (int i = 2; i < count; i++)
        {
            sequence[i] = sequence[i - 1] + sequence[i - 2];
        }

        return sequence;
    }

    /// <summary>
    /// Validates if a given sequence represents a correct Fibonacci sequence
    /// </summary>
    /// <param name="sequence">Comma-separated sequence of numbers to validate</param>
    /// <returns>Validation result with details</returns>
    [KernelFunction]
    [Description("Validates if a sequence is a correct Fibonacci sequence")]
    public string ValidateFibonacci(
        [Description("The sequence to validate (comma-separated numbers)")] string sequence)
    {
        try
        {
            // Parse the input sequence
            var numbers = sequence.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(s => int.Parse(s.Trim()))
                                 .ToArray();

            if (numbers.Length == 0)
                return "❌ INVALID: Empty sequence provided.";

            // Generate the expected Fibonacci sequence of the same length
            var expectedSequence = GenerateFibonacci(numbers.Length);

            // Compare sequences
            bool isValid = numbers.SequenceEqual(expectedSequence);

            if (isValid)
            {
                return $"✅ VALID: The sequence [{string.Join(", ", numbers)}] is a correct Fibonacci sequence.";
            }
            else
            {
                return $"❌ INVALID: The sequence contains errors.\n" +
                       $"Expected: [{string.Join(", ", expectedSequence)}]\n" +
                       $"Provided: [{string.Join(", ", numbers)}]\n" +
                       $"Differences found at positions: {GetDifferencePositions(numbers, expectedSequence)}";
            }
        }
        catch (Exception ex)
        {
            return $"❌ ERROR: Failed to validate sequence. {ex.Message}";
        }
    }

    /// <summary>
    /// Gets the first N Fibonacci numbers as a formatted string
    /// </summary>
    /// <param name="count">Number of Fibonacci numbers to get</param>
    /// <returns>Formatted string of Fibonacci numbers</returns>
    [KernelFunction]
    [Description("Gets the first N Fibonacci numbers as a formatted string")]
    public string GetFibonacciString(
        [Description("The number of Fibonacci numbers to get")] int count)
    {
        var sequence = GenerateFibonacci(count);
        return string.Join(", ", sequence);
    }

    /// <summary>
    /// Checks if a single number could be part of the Fibonacci sequence
    /// </summary>
    /// <param name="number">Number to check</param>
    /// <returns>Whether the number is a Fibonacci number</returns>
    [KernelFunction]
    [Description("Checks if a number is a Fibonacci number")]
    public bool IsFibonacciNumber(
        [Description("The number to check")] int number)
    {
        if (number < 0)
            return false;

        // Generate Fibonacci numbers until we reach or exceed the target
        int a = 0, b = 1;
        
        if (number == a || number == b)
            return true;

        while (b < number)
        {
            int temp = a + b;
            a = b;
            b = temp;
        }

        return b == number;
    }

    private static string GetDifferencePositions(int[] actual, int[] expected)
    {
        var differences = new List<int>();
        
        for (int i = 0; i < Math.Min(actual.Length, expected.Length); i++)
        {
            if (actual[i] != expected[i])
            {
                differences.Add(i);
            }
        }

        if (actual.Length != expected.Length)
        {
            differences.Add(-1); // Indicate length difference
        }

        return differences.Count == 0 
            ? "none" 
            : string.Join(", ", differences.Where(x => x >= 0)) + 
              (differences.Contains(-1) ? " (length mismatch)" : "");
    }
}
