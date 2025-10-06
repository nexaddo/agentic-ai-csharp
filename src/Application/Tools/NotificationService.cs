using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Tools
{
    public class NotificationService(ILogger<NotificationService> logger)
    {
        public Task SendAsync(string toEmail, string ticketKey, string summary, CancellationToken ct = default)
        {
            logger.LogInformation("NOTIFY to={to} ticket={ticket} summary={summary}", toEmail, ticketKey, summary); 
            return Task.CompletedTask;
        }
    }
}