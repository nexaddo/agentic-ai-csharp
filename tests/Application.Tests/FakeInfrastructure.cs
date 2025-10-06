using System.Text.Json;
using Application.DTOs;
using Microsoft.EntityFrameworkCore;
using Application.Tools;
using Application.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Microsoft.Extensions.Configuration;
using Application.Ai;

namespace Application.Tests;

public sealed class FakeAzureOpenAiClient : AzureOpenAiClient
{
    public FakeAzureOpenAiClient() : base(new ConfigurationBuilder().AddInMemoryCollection().Build()) { }

    public new static Task<string> CompleteAsync(string prompt, CancellationToken ct = default)
        => Task.FromResult("""{"intent":"bug_report","priority":"high","summary":"Export to CSV fails with 500"}""");
}

public sealed class FakeJiraClient : JiraClient
{
    public FakeJiraClient() : base(Substitute.For<IHttpClientFactory>(), new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string>
    {
        ["Jira:BaseUrl"]="https://example.atlassian.net",
        ["Jira:Email"]="x@example.com",
        ["Jira:ApiToken"]="secret",
        ["Jira:ProjectKey"]="IT"
    }).Build()) {}

    public new static Task<string> CreateTicketAsync(string summary, string description, string priority, CancellationToken ct = default)
        => Task.FromResult<string>("IT-123");
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
