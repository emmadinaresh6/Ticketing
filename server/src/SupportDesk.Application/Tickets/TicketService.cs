using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstractions;
using SupportDesk.Application.Common;
using SupportDesk.Domain;
using SupportDesk.Domain.Entities;

namespace SupportDesk.Application.Tickets;

// All queries go through the filtered DbSets, so a caller only ever sees tickets they are allowed to.
// A ticket the caller cannot see comes back as null, which the API returns as 404 (never 403).
public class TicketService(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    ITicketNumberGenerator numbers,
    TimeProvider clock)
{
    public async Task<TicketSummaryDto> CreateAsync(CreateTicketRequest request, CancellationToken ct)
    {
        var userId = RequireUser();

        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct)
            ?? throw new AppValidationException("Category does not exist.");

        var now = clock.GetUtcNow();
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Number = await numbers.NextAsync(currentUser.Origin, ct),
            Origin = currentUser.Origin,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            CategoryId = category.Id,
            Priority = request.Priority,
            Severity = request.Severity,
            Status = TicketStatus.New,
            RequesterId = userId,
            CreatedAt = now
        };

        db.Tickets.Add(ticket);
        db.TicketHistory.Add(new TicketHistory
        {
            TicketId = ticket.Id,
            Action = "Created",
            NewValue = nameof(TicketStatus.New),
            ChangedById = userId,
            ChangedAt = now
        });
        db.AuditLogs.Add(new AuditLog { UserId = userId, TicketId = ticket.Id, Action = "Created", Timestamp = now });
        await db.SaveChangesAsync(ct);

        return new TicketSummaryDto(ticket.Id, ticket.Number, ticket.Origin, ticket.Title, ticket.Status,
            ticket.Priority, ticket.Severity, category.Name, ticket.CreatedAt);
    }

    // Scope narrows what the caller can already see; it never widens it (the access filter still applies).
    public async Task<IReadOnlyList<TicketSummaryDto>> ListAsync(TicketScope scope, TicketStatus? status, CancellationToken ct)
    {
        var userId = RequireUser();

        var query = scope switch
        {
            TicketScope.Mine => db.Tickets.Where(t => t.RequesterId == userId),
            TicketScope.Queue => db.Tickets.Where(t => t.RequesterId != userId),
            _ => db.Tickets.AsQueryable()
        };
        if (status is not null)
        {
            query = query.Where(t => t.Status == status);
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Take(200)
            .Join(db.Categories, t => t.CategoryId, c => c.Id, (t, c) => new TicketSummaryDto(
                t.Id, t.Number, t.Origin, t.Title, t.Status, t.Priority, t.Severity, c.Name, t.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<TicketDetailDto?> GetAsync(Guid id, CancellationToken ct)
    {
        var userId = RequireUser();

        var ticket = await db.Tickets
            .AsNoTracking()
            .Include(t => t.Comments.OrderBy(c => c.CreatedAt))
            .FirstOrDefaultAsync(t => t.Id == id, ct);
        if (ticket is null)
        {
            return null;
        }

        // Every view is audited.
        db.AuditLogs.Add(new AuditLog { UserId = userId, TicketId = ticket.Id, Action = "Viewed", Timestamp = clock.GetUtcNow() });
        await db.SaveChangesAsync(ct);

        var userIds = ticket.Comments.Select(c => c.AuthorId).Append(ticket.RequesterId).ToList();
        if (ticket.AssignedUserId is { } assignedId)
        {
            userIds.Add(assignedId);
        }
        var names = await db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);
        var categoryName = await db.Categories.Where(c => c.Id == ticket.CategoryId).Select(c => c.Name).FirstAsync(ct);
        var groupName = ticket.AssignedGroupId is null
            ? null
            : await db.SpecialistGroups.Where(g => g.Id == ticket.AssignedGroupId).Select(g => g.Name).FirstOrDefaultAsync(ct);

        return new TicketDetailDto(
            ticket.Id, ticket.Number, ticket.Origin, ticket.Title, ticket.Description, ticket.Status,
            ticket.Priority, ticket.Severity, ticket.CategoryId, categoryName,
            names.GetValueOrDefault(ticket.RequesterId, "Unknown"),
            groupName,
            ticket.AssignedUserId is { } assignedUser ? names.GetValueOrDefault(assignedUser) : null,
            ticket.CreatedAt,
            ticket.Comments
                .Select(c => new CommentDto(c.Id, names.GetValueOrDefault(c.AuthorId, "Unknown"), c.Body, c.IsInternal, c.CreatedAt))
                .ToList());
    }

    public async Task<CommentDto?> AddCommentAsync(Guid ticketId, AddCommentRequest request, CancellationToken ct)
    {
        var userId = RequireUser();

        var exists = await db.Tickets.AnyAsync(t => t.Id == ticketId, ct);
        if (!exists)
        {
            return null;
        }

        if (request.IsInternal && !currentUser.IsStaff)
        {
            throw new AppValidationException("Only specialists and admins can add internal notes.");
        }

        var comment = new TicketComment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            AuthorId = userId,
            Body = request.Body.Trim(),
            IsInternal = request.IsInternal,
            CreatedAt = clock.GetUtcNow()
        };
        db.TicketComments.Add(comment);
        await db.SaveChangesAsync(ct);

        var author = await db.Users.Where(u => u.Id == userId).Select(u => u.DisplayName).FirstAsync(ct);
        return new CommentDto(comment.Id, author, comment.Body, comment.IsInternal, comment.CreatedAt);
    }

    private Guid RequireUser() =>
        currentUser.UserId ?? throw new InvalidOperationException("No signed-in user for this request.");
}
