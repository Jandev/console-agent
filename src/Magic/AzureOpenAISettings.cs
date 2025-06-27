namespace Magic;

/// <summary>
/// Configuration settings for Azure OpenAI
/// </summary>
public class AzureOpenAISettings
{
    public const string SectionName = "AzureOpenAI";
    
    public string Endpoint { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
