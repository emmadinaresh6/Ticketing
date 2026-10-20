using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Categories;
using SupportDesk.Application.Tickets;

namespace SupportDesk.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<TicketService>();
        services.AddScoped<CategoryService>();
        return services;
    }
}
