using System.Threading;
using System.Threading.Tasks;

namespace Application.Tools
{
    public interface IJiraClient
    {
        Task<string> CreateTicketAsync(string summary, string description, string priority, CancellationToken ct = default);
    }
}