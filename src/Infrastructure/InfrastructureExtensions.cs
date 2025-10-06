using Application.Abstractions;
using Application.Ai;
using Application.Persistence;
using Application.Services;
using Application.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Infrastructure
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(o => o.UseSqlite(config.GetConnectionString("Sqlite") ?? "Data Source=support.db")); 
            services.AddHttpClient(); services.AddSingleton<AzureOpenAiClient>(); services.AddSingleton<JiraClient>(); 
            services.AddSingleton<NotificationService>(); services.AddScoped<IAgentService, AgentService>(); 
            using var scope = services.BuildServiceProvider().CreateScope(); 
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); 
            db.Database.EnsureCreated(); 
            
            return services;
        }
    }
}