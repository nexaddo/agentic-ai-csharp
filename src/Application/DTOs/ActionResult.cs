namespace Application.DTOs 
{ 
    public sealed record ActionResultDto(string Status, string TicketKey, AgentDecision Decision, string Notes = null); 
}