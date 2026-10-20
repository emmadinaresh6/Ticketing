using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Infrastructure.Persistence;

namespace SupportDesk.IntegrationTests;

// Runs the real API against a throwaway LocalDB database, rebuilt from migrations for each test run.
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=SupportDesk_Tests;Trusted_Connection=True;TrustServerCertificate=True";

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiFactory()
    {
        // Environment variables override appsettings and are read when Program builds its configuration.
        Environment.SetEnvironmentVariable("ConnectionStrings__SupportDesk", TestConnectionString);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Development");

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupportDeskDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    public async Task<HttpClient> ClientForAsync(string devUser)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/dev/login", new { user = devUser });
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(Json);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        return client;
    }

    // Test-only data change that bypasses the access filters (assignment endpoints come in epic 5).
    public async Task AssignAsync(Guid ticketId, Guid? groupId, Guid? userId = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupportDeskDbContext>();
        var ticket = await db.Tickets.IgnoreQueryFilters().SingleAsync(t => t.Id == ticketId);
        ticket.AssignedGroupId = groupId;
        ticket.AssignedUserId = userId;
        await db.SaveChangesAsync();
    }

    public async Task<int> CountAuditAsync(Guid ticketId, string action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupportDeskDbContext>();
        return await db.AuditLogs.IgnoreQueryFilters().CountAsync(a => a.TicketId == ticketId && a.Action == action);
    }

    private record LoginResponse(string AccessToken);
}

[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}
