using Application.Abstractions;
using Application.DTOs;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Persistence;
using Application.Ai;
using Application.Tools;

namespace Application.Services
{
    public sealed class AgentService(AzureOpenAiChatClient ai, JiraClient jira, NotificationService notify, AppDbContext db) : IAgentService
    {
        readonly AzureOpenAiChatClient _ai = ai;
        readonly JiraClient _jira = jira;
        readonly NotificationService _notify = notify;
        readonly AppDbContext _db = db;

        public async Task<AgentDecision> ClassifyAsync(InboundRequest request, CancellationToken ct = default)
        {
            var prompt = $@"
            You are an expert support triage agent. Classify the message into: intent [password_reset, bug_report, feature_request, other], priority [low, medium, high], and provide a single-sentence summary. Return strict JSON: {{""intent"":""..."", ""priority"":""..."", ""summary"":""...""}}. Message: 
            """"{request.Message}""""
            ";
            var raw = await _ai.CompleteAsync(prompt, ct);
            JsonElement root;
            try
            {
                using var d = JsonDocument.Parse(raw);
                root = d.RootElement.Clone();
            }
            catch
            {
                var s = raw.IndexOf('{');
                var e = raw.LastIndexOf('}');
                if (s >= 0 && e > s)
                {
                    using var d = JsonDocument.Parse(raw[s..(e + 1)]);
                    root = d.RootElement.Clone();
                }
                else
                {
                    using var d = JsonDocument.Parse("{\"intent\":\"other\",\"priority\":\"low\",\"summary\":\"Unparsed\"}");
                    root = d.RootElement.Clone();
                }
            }
            return new AgentDecision(root.GetProperty("intent").GetString() ?? "other", root.GetProperty("priority").GetString() ?? "low", root.GetProperty("summary").GetString() ?? "Request");
        }
        public async Task<ActionResultDto> ActAsync(InboundRequest request, CancellationToken ct = default)
        {
            var decision = await ClassifyAsync(request, ct);
            if (!decision.CreateTicket)
            {
                return new ActionResultDto("skipped", null, decision);
            }
            var description = $"Requester: {request.RequesterEmail ?? "unknown"}\n\nOriginal Message:\n{request.Message}";
            var key = await _jira.CreateTicketAsync(decision.Summary, description, decision.Priority, ct);
            if (key is null)
            {
                return new ActionResultDto("failed", null, decision, "Jira creation failed");
            }
            _db.Tickets.Add(new Ticket
            {
                JiraKey = key,
                RequesterEmail = request.RequesterEmail,
                Summary = decision.Summary,
                Intent = decision.Intent,
                Priority = decision.Priority,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync(ct);
            if (!string.IsNullOrWhiteSpace(request.RequesterEmail))
            {
                await _notify.SendAsync(request.RequesterEmail!, key, decision.Summary, ct);
            }
            return new ActionResultDto("created", key, decision);
        }
    }
}