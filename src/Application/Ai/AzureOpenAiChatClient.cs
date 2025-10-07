
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Ai;

public class AzureOpenAiChatClient
{
    private readonly AzureOpenAIClient _client;
    private readonly string _deployment;

    public AzureOpenAiChatClient(IConfiguration cfg)
    {
        var ep = cfg["AzureOpenAI:Endpoint"]!;
        var key = cfg["AzureOpenAI:ApiKey"]!;
        _deployment = cfg["AzureOpenAI:Deployment"]!;
        _client = new AzureOpenAIClient(new Uri(ep), new AzureKeyCredential(key));
    }

    public async Task<string> CompleteAsync(string prompt, CancellationToken ct = default)
    {
        var chatClient = _client.GetChatClient(_deployment);
        var messages = new List<ChatMessage>()
        {
            new SystemChatMessage("You are a precise JSON-only assistant."),
            new UserChatMessage(prompt),
        };

        var response = await chatClient.CompleteChatAsync(messages, new ChatCompletionOptions()
        {
            Temperature = (float)0.7,
            FrequencyPenalty = (float)0,
            PresencePenalty = (float)0,
        }, ct);
        return response.Value.Content.Last().Text ?? "{}";
    }
}
