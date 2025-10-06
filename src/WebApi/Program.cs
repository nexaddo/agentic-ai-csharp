using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

var b = WebApplication.CreateBuilder(args);
if (System.IO.Directory.Exists("/mnt/kv"))
{
    b.Configuration.AddKeyPerFile("/mnt/kv", true);
}
b.Services.AddInfrastructure(b.Configuration);
b.Services.AddControllers();
b.Services.AddEndpointsApiExplorer();
b.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "AI Support Agent", Version = "v1" }));
var app = b.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapGet("/healthz", () => Results.Ok(new { ok = true }));
app.Run();