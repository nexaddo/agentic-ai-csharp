using Application.Abstractions;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/agent")]
    public class AgentController(IAgentService agent) : ControllerBase
    {
        readonly IAgentService _agent = agent;

        [HttpPost("classify")]
        public async Task<IActionResult> Classify([FromBody] InboundRequest req, CancellationToken ct) 
            => Ok(await _agent.ClassifyAsync(req, ct));
        
        [HttpPost("act")]
        public async Task<IActionResult> Act([FromBody] InboundRequest req, CancellationToken ct)
            => Ok(await _agent.ActAsync(req, ct));
    }
}