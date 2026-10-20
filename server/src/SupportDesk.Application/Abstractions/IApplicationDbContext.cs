using Microsoft.EntityFrameworkCore;
using SupportDesk.Domain;
using SupportDesk.Domain.Entities;

namespace SupportDesk.Application.Abstractions;

// Every DbSet here is subject to the global access filters in Infrastructure.
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<SpecialistGroup> SpecialistGroups { get; }
    DbSet<Category> Categories { get; }
    DbSet<Ticket> Tickets { get; }
    DbSet<TicketComment> TicketComments { get; }
    DbSet<TicketHistory> TicketHistory { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ITicketNumberGenerator
{
    Task<string> NextAsync(Origin origin, CancellationToken cancellationToken);
}
