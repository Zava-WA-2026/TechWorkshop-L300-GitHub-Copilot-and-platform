using Azure;
using Azure.AI.OpenAI;
using Azure.AI.ContentSafety;
using Azure.Identity;
using OpenAI.Chat;
using ZavaStorefront.Models;

namespace ZavaStorefront.Services;

public interface IChatService
{
    Task<ChatResponse> GetResponseAsync(ChatRequest request);
}

public class ChatService : IChatService
{
    private readonly AzureOpenAIClient _client;
    private readonly ContentSafetyClient _contentSafetyClient;
    private readonly ILogger<ChatService> _logger;
    private readonly string _modelName;
    private const int UnsafeSeverityThreshold = 2;

    public ChatService(IConfiguration configuration, ILogger<ChatService> logger)
    {
        _logger = logger;
        
        var endpoint = configuration["AzureAI:Endpoint"] 
            ?? throw new InvalidOperationException("AzureAI:Endpoint is not configured");
        var contentSafetyEndpoint = configuration["AzureAI:ContentSafetyEndpoint"] 
            ?? "https://aizavadevdyu3e.cognitiveservices.azure.com/";
        _modelName = configuration["AzureAI:ModelName"] ?? "gpt-4o-mini";

        var credential = new DefaultAzureCredential();

        // Use DefaultAzureCredential for managed identity authentication
        _client = new AzureOpenAIClient(
            new Uri(endpoint),
            credential);
        
        // Initialize Content Safety client using the Cognitive Services endpoint
        _contentSafetyClient = new ContentSafetyClient(
            new Uri(contentSafetyEndpoint),
            credential);
        
        _logger.LogInformation("ChatService initialized with managed identity authentication and content safety");
    }

    /// <summary>
    /// Evaluates user input for content safety violations.
    /// Returns a tuple indicating if the content is safe and any warning message.
    /// </summary>
    private async Task<(bool IsSafe, string? WarningMessage)> EvaluateContentSafetyAsync(string userMessage)
    {
        try
        {
            var request = new AnalyzeTextOptions(userMessage);
            
            var response = await _contentSafetyClient.AnalyzeTextAsync(request);
            
            var categories = new Dictionary<string, int?>
            {
                { "Violence", response.Value.CategoriesAnalysis.FirstOrDefault(c => c.Category == TextCategory.Violence)?.Severity },
                { "Sexual", response.Value.CategoriesAnalysis.FirstOrDefault(c => c.Category == TextCategory.Sexual)?.Severity },
                { "SelfHarm", response.Value.CategoriesAnalysis.FirstOrDefault(c => c.Category == TextCategory.SelfHarm)?.Severity },
                { "Hate", response.Value.CategoriesAnalysis.FirstOrDefault(c => c.Category == TextCategory.Hate)?.Severity }
            };
            
            // Log all category scores
            _logger.LogInformation(
                "ContentSafety evaluation - Violence: {Violence}, Sexual: {Sexual}, SelfHarm: {SelfHarm}, Hate: {Hate}",
                categories["Violence"] ?? 0,
                categories["Sexual"] ?? 0,
                categories["SelfHarm"] ?? 0,
                categories["Hate"] ?? 0);
            
            // Check if any category exceeds the threshold
            foreach (var category in categories)
            {
                if (category.Value.HasValue && category.Value.Value >= UnsafeSeverityThreshold)
                {
                    _logger.LogWarning(
                        "ContentSafety violation detected - Category: {Category}, Severity: {Severity}, Threshold: {Threshold}",
                        category.Key,
                        category.Value.Value,
                        UnsafeSeverityThreshold);
                    
                    return (false, $"Your message was flagged for potentially unsafe content ({category.Key}). Please rephrase your question.");
                }
            }
            
            _logger.LogInformation("ContentSafety check passed - Message is safe");
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ContentSafety evaluation failed - Error: {ErrorMessage}", ex.Message);
            // If content safety check fails, we'll allow the message but log the error
            // In production, you might want to block instead
            return (true, null);
        }
    }

    public async Task<ChatResponse> GetResponseAsync(ChatRequest request)
    {
        try
        {
            // Step 1: Evaluate content safety before processing
            var (isSafe, warningMessage) = await EvaluateContentSafetyAsync(request.Message);
            
            if (!isSafe)
            {
                _logger.LogWarning("ContentSafety blocked message - User message was flagged as unsafe");
                return new ChatResponse
                {
                    Success = false,
                    Error = warningMessage ?? "Your message contains content that violates our safety guidelines. Please try a different question."
                };
            }
            
            // Step 2: Process safe messages through the AI model
            var chatClient = _client.GetChatClient(_modelName);
            
            var messages = new List<OpenAI.Chat.ChatMessage>
            {
                new SystemChatMessage("You are a helpful AI assistant for Zava Storefront. Help customers with questions about products, pricing, and general inquiries. Be friendly and professional.")
            };

            // Add conversation history
            foreach (var msg in request.History)
            {
                if (msg.Role.ToLower() == "user")
                    messages.Add(new UserChatMessage(msg.Content));
                else if (msg.Role.ToLower() == "assistant")
                    messages.Add(new AssistantChatMessage(msg.Content));
            }

            // Add the current message
            messages.Add(new UserChatMessage(request.Message));

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 1000,
                Temperature = 0.7f
            };

            var response = await chatClient.CompleteChatAsync(messages, options);

            return new ChatResponse
            {
                Response = response.Value.Content[0].Text,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat response");
            return new ChatResponse
            {
                Success = false,
                Error = "Sorry, I'm having trouble responding right now. Please try again later."
            };
        }
    }
}
