using Application.DTOs;
using Application.Services;
using Application.Tools;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace Application.Tests;

public class AgentServiceTests
{
    [Fact]
    public async Task ClassifyAsync_Returns_Expected_AgentDecision()
    {
        // arrange
        var ai = new FakeAzureOpenAiClient();
        var jira = new FakeJiraClient();
        var notify = new FakeNotify();
        using var db = InMemoryDb.Create();

        var svc = new AgentService(ai, jira, notify, db);
        var req = new InboundRequest("alex@example.com", "The CSV export throws a 500 after the latest release.");

        // act
        var decision = await svc.ClassifyAsync(req);

        // assert
        decision.Intent.Should().Be("bug_report");
        decision.Priority.Should().Be("high");
        decision.Summary.Should().Contain("CSV");
    }

    [Fact]
    public async Task ActAsync_Creates_Ticket_Persists_And_Notifies()
    {
        // arrange
        var ai = new FakeAzureOpenAiClient();
        var jira = new FakeJiraClient();
        var notify = new FakeNotify();
        using var db = InMemoryDb.Create();

        var svc = new AgentService(ai, jira, notify, db);
        var req = new InboundRequest("alex@example.com", "The CSV export throws a 500 after the latest release.");

        // act
        var result = await svc.ActAsync(req);

        // assert
        result.Status.Should().Be("created");
        result.TicketKey.Should().Be("IT-123");
        db.Tickets.Count().Should().Be(1);
        db.Tickets.Single().Summary.Should().Contain("CSV");
    }

    [Fact]
    public async Task ActAsync_Gracefully_Fails_When_Jira_Returns_Null()
    {
        // arrange: Jira returns null
        var ai = new FakeAzureOpenAiClient();
        var jira = Substitute.For<JiraClient>(
            Substitute.For<IHttpClientFactory>(),
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>
            {
                ["Jira:BaseUrl"] = "https://example.atlassian.net",
                ["Jira:Email"] = "x@example.com",
                ["Jira:ApiToken"] = "secret",
                ["Jira:ProjectKey"] = "IT"
            }).Build()
        );
        jira.CreateTicketAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((string?)null);

        var notify = new FakeNotify();
        using var db = InMemoryDb.Create();

        var svc = new AgentService(ai, jira, notify, db);
        var req = new InboundRequest("alex@example.com", "The CSV export throws a 500.");

        // act
        var result = await svc.ActAsync(req);

        // assert
        result.Status.Should().Be("failed");
        result.TicketKey.Should().BeNull();
        db.Tickets.Count().Should().Be(0);
    }
}
