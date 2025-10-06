using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Tools
{
    public sealed class JiraClient
    {
        readonly HttpClient _http; readonly string _baseUrl;
        readonly string _projectKey;
        public JiraClient(IHttpClientFactory f, IConfiguration cfg)
        {
            _http = f.CreateClient();
            _baseUrl = cfg["Jira:BaseUrl"];
            _projectKey = cfg["Jira:ProjectKey"] ?? "IT";
            var email = cfg["Jira:Email"];
            var token = cfg["Jira:ApiToken"];
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(token))
            {
                var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{token}"));
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", b64);
            }
        }
        public async Task<string> CreateTicketAsync(string summary, string description, string priority, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_baseUrl))
                return "IT-LOCAL-1";
            var url = $"{_baseUrl}/rest/api/3/issue";
            var payload = new
            {
                fields = new
                {
                    project = new { key = _projectKey },
                    summary = summary.Length > 240 ? summary[..240] : summary,
                    description,
                    issuetype = new { name = "Task" },
                    priority = new
                    {
                        name = ToTitle(priority)
                    }
                }
            };
            var json = JsonSerializer.Serialize(payload);
            var res = await _http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"), ct);
            if (!res.IsSuccessStatusCode) return null; using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct)); 
            return doc.RootElement.TryGetProperty("key", out var key) ? key.GetString() : null;
        }
        static string ToTitle(string s) => string.IsNullOrWhiteSpace(s) ? "Low" : char.ToUpper(s[0]) + s[1..].ToLower();
    }
}