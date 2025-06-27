# Azure OpenAI Configuration Setup

This application has been updated to use Azure OpenAI instead of the mock chat completion service.

## Configuration Files

### appsettings.json
Contains non-sensitive configuration like endpoint and deployment name:
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource-name.openai.azure.com/",
    "DeploymentName": "gpt-4"
  }
}
```

### User Secrets (for API Key)
The API key is stored securely using .NET user secrets. To set it up:

1. Navigate to the project directory:
   ```powershell
   cd src/Magic
   ```

2. Set the API key using user secrets:
   ```powershell
   dotnet user-secrets set "AzureOpenAI:ApiKey" "your-actual-api-key-here"
   ```

## Required Configuration Steps

1. **Update appsettings.json**: Replace the values in `appsettings.json` with your actual Azure OpenAI resource information:
   - `Endpoint`: Your Azure OpenAI resource endpoint
   - `DeploymentName`: The name of your deployed model (e.g., "gpt-4", "gpt-35-turbo")

2. **Set API Key**: Use the user secrets command above to securely store your API key

3. **Verify Configuration**: The application will validate the configuration on startup and provide helpful error messages if anything is missing.

## Benefits of This Approach

- **Security**: API key is stored securely in user secrets, not in source code
- **Configuration Management**: Non-sensitive settings in appsettings.json for easy management
- **Environment Separation**: Different configurations can be used for development, staging, and production
- **Real AI Integration**: Uses actual Azure OpenAI service instead of mock responses

## Mock Service Preserved

The `MockChatCompletionService.cs` file has been preserved on disk for reference but is no longer used by the application.
