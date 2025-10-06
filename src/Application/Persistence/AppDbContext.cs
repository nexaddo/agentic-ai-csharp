using Microsoft.EntityFrameworkCore;
using System;
namespace Application.Persistence
{
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Ticket> Tickets => Set<Ticket>();
    }
    public sealed class Ticket
    {
        public int Id { get; set; }
        public string JiraKey { get; set; }
        public string RequesterEmail { get; set; }
        public string Summary { get; set; } = ""; 
        public string Intent { get; set; } = "other"; 
        public string Priority { get; set; } = "low"; 
        public DateTime CreatedAt { get; set; }
    }
}