using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Abstractions;
using SupportDesk.Infrastructure.Persistence;

namespace SupportDesk.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SupportDeskDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<SupportDeskDbContext>());
        services.AddScoped<ITicketNumberGenerator, TicketNumberGenerator>();
        return services;
    }
}
