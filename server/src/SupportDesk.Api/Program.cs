using System.Text.Json.Serialization;
using SupportDesk.Api;
using SupportDesk.Api.Auth;
using SupportDesk.Application;
using SupportDesk.Infrastructure;
using SupportDesk.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SupportDesk")
    ?? throw new InvalidOperationException("Connection string 'SupportDesk' is not configured.");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddSupportDeskAuth(builder.Environment);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks().AddDbContextCheck<SupportDeskDbContext>("database");

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<CurrentUserMiddleware>();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapDevAuth();
}

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

public partial class Program;
