using Application.DTOs;
using System.Threading;
using System.Threading.Tasks;
namespace Application.Abstractions
{
    public interface IAgentService
    {
        Task<AgentDecision> ClassifyAsync(InboundRequest request, CancellationToken ct = default);
        Task<ActionResultDto> ActAsync(InboundRequest request, CancellationToken ct = default);
    }
}