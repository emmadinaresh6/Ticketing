using System.ComponentModel.DataAnnotations;
using SupportDesk.Domain;

namespace SupportDesk.Application.Tickets;

public enum TicketScope
{
    // Everything the caller can see.
    All = 0,
    // Tickets the caller raised.
    Mine = 1,
    // Tickets raised by others that the caller can see (assigned to them or their groups; all for admins).
    Queue = 2
}

public record CreateTicketRequest(
    [Required, MaxLength(200)] string Title,
    [Required, MaxLength(8000)] string Description,
    [Required] Guid CategoryId,
    [Required] Priority Priority,
    [Required] Severity Severity);

public record AddCommentRequest(
    [Required, MaxLength(8000)] string Body,
    bool IsInternal = false);

public record TicketSummaryDto(
    Guid Id,
    string Number,
    Origin Origin,
    string Title,
    TicketStatus Status,
    Priority Priority,
    Severity Severity,
    string CategoryName,
    DateTimeOffset CreatedAt);

public record CommentDto(
    Guid Id,
    string AuthorName,
    string Body,
    bool IsInternal,
    DateTimeOffset CreatedAt);

public record TicketDetailDto(
    Guid Id,
    string Number,
    Origin Origin,
    string Title,
    string Description,
    TicketStatus Status,
    Priority Priority,
    Severity Severity,
    Guid CategoryId,
    string CategoryName,
    string RequesterName,
    string? AssignedGroupName,
    string? AssignedUserName,
    DateTimeOffset CreatedAt,
    IReadOnlyList<CommentDto> Comments);
