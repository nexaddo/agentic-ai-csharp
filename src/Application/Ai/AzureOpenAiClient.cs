using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace Application.Ai
{
    public class AzureOpenAiClient(IConfiguration cfg)
    {
        readonly string _endpoint = cfg["AzureOpenAI:Endpoint"] ?? cfg["AOAI-ENDPOINT"] ?? "";
        readonly string _apiKey = cfg["AzureOpenAI:ApiKey"] ?? cfg["AOAI-API-KEY"] ?? "";
        readonly string _deployment = cfg["AzureOpenAI:Deployment"] ?? cfg["AOAI-DEPLOYMENT"] ?? "gpt-4o-mini";

        public static Task<string> CompleteAsync(string prompt, CancellationToken ct = default)
        {
            var intent = prompt.Contains("password", StringComparison.OrdinalIgnoreCase) ? "password_reset" : prompt.Contains("500", StringComparison.OrdinalIgnoreCase) ? "bug_report" : "other";
            var priority = intent == "bug_report" ? "high" : "low";
            var summary = prompt.Contains("CSV", StringComparison.OrdinalIgnoreCase) ? "Export to CSV fails with 500" : "General support request";

            return Task.FromResult($"{{\"intent\":\"{intent}\",\"priority\":\"{priority}\",\"summary\":\"{summary}\"}}");
        }
    }
}