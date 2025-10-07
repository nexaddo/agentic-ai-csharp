using Application.Ai;
using Application.DTOs;
using Application.Persistence;
using Application.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.Json;
using Xunit.Abstractions;

namespace Application.Tests;

public sealed class FakeAzureOpenAiClient : AzureOpenAiChatClient
{
    public FakeAzureOpenAiClient() : base(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["AzureOpenAI:Endpoint"] = "https://example.openai.azure.com/",
        ["AzureOpenAI:ApiKey"] = "x",
        ["AzureOpenAI:Deployment"] = "gpt-4o-mini"
    }).Build())
    { }
    public new static Task<string> CompleteAsync(string prompt, CancellationToken ct = default)
            => Task.FromResult("""{"intent":"bug_report","priority":"high","summary":"Export to CSV fails with 500"}""");
}

public sealed class FakeJiraClient : JiraClient
{
    public FakeJiraClient(IHttpClientFactory clientFactory) : base(clientFactory, new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string>
    {
        ["Jira:BaseUrl"]="https://example.atlassian.net",
        ["Jira:Email"]="x@example.com",
        ["Jira:ApiToken"]="secret",
        ["Jira:ProjectKey"]="IT"
    }).Build()) 
    {}
}

public sealed class FakeNotify : NotificationService
{
    public FakeNotify() : base(Substitute.For<ILogger<NotificationService>>()) { }
}

public static class InMemoryDb
{
    public static AppDbContext Create()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(opts);
    }
}
