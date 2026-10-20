namespace SupportDesk.Domain.Entities;

public class Ticket
{
    public Guid Id { get; set; }

    // INT-000123 or EXT-000123, generated from a database sequence per origin.
    public string Number { get; set; } = string.Empty;
    public Origin Origin { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Priority Priority { get; set; }
    public Severity Severity { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.New;
    public Guid RequesterId { get; set; }
    public Guid? AssignedGroupId { get; set; }
    public Guid? AssignedUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? SlaDueAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }

    public List<TroubleshootingStep> Steps { get; set; } = [];
    public List<TicketComment> Comments { get; set; } = [];
    public List<Attachment> Attachments { get; set; } = [];
}

public class TroubleshootingStep
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public int Sequence { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public Guid? KbArticleId { get; set; }
    public StepOutcome Outcome { get; set; } = StepOutcome.Pending;
    public string? OutcomeNote { get; set; }
    public DateTimeOffset? RecordedAt { get; set; }
}

public class TicketComment
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid AuthorId { get; set; }
    public string Body { get; set; } = string.Empty;

    // Never returned to requesters.
    public bool IsInternal { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class Attachment
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string BlobPath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public ScanStatus ScanStatus { get; set; } = ScanStatus.Pending;
    public Guid UploadedById { get; set; }
}

public class Resolution
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string RootCause { get; set; } = string.Empty;
    public bool SafeToReuse { get; set; }
    public Guid? ReviewedById { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
