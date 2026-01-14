using Azure;
using Azure.AI.Inference;
using ZavaStorefront.Models;

namespace ZavaStorefront.Services;

public interface IChatService
{
    Task<ChatResponse> GetResponseAsync(ChatRequest request);
}

public class ChatService : IChatService
{
    private readonly ChatCompletionsClient _client;
    private readonly ILogger<ChatService> _logger;
    private readonly string _modelName;

    public ChatService(IConfiguration configuration, ILogger<ChatService> logger)
    {
        _logger = logger;
        
        var endpoint = configuration["AzureAI:Endpoint"] 
            ?? throw new InvalidOperationException("AzureAI:Endpoint is not configured");
        var apiKey = configuration["AzureAI:ApiKey"] 
            ?? throw new InvalidOperationException("AzureAI:ApiKey is not configured");
        _modelName = configuration["AzureAI:ModelName"] ?? "Phi-4";

        _client = new ChatCompletionsClient(
            new Uri(endpoint),
            new AzureKeyCredential(apiKey));
    }

    public async Task<ChatResponse> GetResponseAsync(ChatRequest request)
    {
        try
        {
            var messages = new List<ChatRequestMessage>
            {
                new ChatRequestSystemMessage("You are a helpful AI assistant for Zava Storefront. Help customers with questions about products, pricing, and general inquiries. Be friendly and professional.")
            };

            // Add conversation history
            foreach (var msg in request.History)
            {
                if (msg.Role.ToLower() == "user")
                    messages.Add(new ChatRequestUserMessage(msg.Content));
                else if (msg.Role.ToLower() == "assistant")
                    messages.Add(new ChatRequestAssistantMessage(msg.Content));
            }

            // Add the current message
            messages.Add(new ChatRequestUserMessage(request.Message));

            var options = new ChatCompletionsOptions(messages)
            {
                Model = _modelName,
                MaxTokens = 1000,
                Temperature = 0.7f
            };

            var response = await _client.CompleteAsync(options);

            return new ChatResponse
            {
                Response = response.Value.Content,
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
