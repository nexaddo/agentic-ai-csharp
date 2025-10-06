namespace Application.DTOs 
{ 
    public sealed record InboundRequest(string RequesterEmail, string Message);
}