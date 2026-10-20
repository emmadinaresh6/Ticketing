namespace SupportDesk.Domain;

public enum Origin
{
    Internal = 1,
    External = 2
}

public enum RoleName
{
    Requester = 1,
    Specialist = 2,
    Admin = 3
}

public enum TicketStatus
{
    New = 1,
    Assigned = 2,
    InProgress = 3,
    WaitingOnUser = 4,
    Resolved = 5,
    Closed = 6,
    SelfResolved = 7,
    Reopened = 8
}

public enum Priority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

// Placeholder values; final list is an open item. Does not affect SLA.
public enum Severity
{
    Minor = 1,
    Moderate = 2,
    Major = 3,
    Critical = 4
}

public enum StepOutcome
{
    Pending = 0,
    Worked = 1,
    DidNotWork = 2,
    CouldNotTry = 3
}

public enum ScanStatus
{
    Pending = 0,
    Clean = 1,
    Infected = 2,
    Failed = 3
}

public enum KbSourceType
{
    Manual = 1,
    Resolution = 2
}

public enum NotificationChannel
{
    Email = 1
}
