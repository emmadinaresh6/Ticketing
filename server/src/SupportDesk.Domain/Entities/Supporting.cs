namespace SupportDesk.Domain.Entities;

public class AgentRun
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string PromptVersion { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string RedactedInput { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public class KbArticle
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public KbSourceType SourceType { get; set; }
    public Guid? ResolutionId { get; set; }
    public bool SafeToReuse { get; set; }
}

public class SlaPolicy
{
    public Guid Id { get; set; }
    public Origin Origin { get; set; }
    public Priority Priority { get; set; }
    public int ResponseMinutes { get; set; }
    public int ResolutionMinutes { get; set; }
}

// Append-only.
public class TicketHistory
{
    public long Id { get; set; }
    public Guid TicketId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public Guid ChangedById { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
}

// Append-only. Admins only. Includes every ticket view.
public class AuditLog
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? TicketId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TicketId { get; set; }
    public NotificationChannel Channel { get; set; }
    public DateTimeOffset SentAt { get; set; }
}
