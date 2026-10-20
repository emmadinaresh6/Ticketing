using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstractions;
using SupportDesk.Domain.Entities;

namespace SupportDesk.Infrastructure.Persistence;

public class SupportDeskDbContext(DbContextOptions<SupportDeskDbContext> options, ICurrentUser currentUser)
    : DbContext(options), IApplicationDbContext
{
    // Read by the global query filters. EF re-evaluates these per query, so each request
    // sees data for its own caller. No signed-in user means Guid.Empty and no roles: nothing is visible.
    private Guid CurrentUserId => currentUser.UserId ?? Guid.Empty;
    private bool CurrentUserIsAdmin => currentUser.IsAdmin;
    private bool CurrentUserIsSpecialist => currentUser.IsSpecialist;
    private bool CurrentUserIsStaff => currentUser.IsStaff;
    private IReadOnlyList<Guid> CurrentUserGroupIds => currentUser.GroupIds;

    public const string InternalNumberSequence = "TicketNumbers_Internal";
    public const string ExternalNumberSequence = "TicketNumbers_External";

    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<SpecialistGroup> SpecialistGroups => Set<SpecialistGroup>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TroubleshootingStep> TroubleshootingSteps => Set<TroubleshootingStep>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Resolution> Resolutions => Set<Resolution>();
    public DbSet<AgentRun> AgentRuns => Set<AgentRun>();
    public DbSet<KbArticle> KbArticles => Set<KbArticle>();
    public DbSet<SlaPolicy> SlaPolicies => Set<SlaPolicy>();
    public DbSet<TicketHistory> TicketHistory => Set<TicketHistory>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>(InternalNumberSequence).StartsAt(1).IncrementsBy(1);
        modelBuilder.HasSequence<long>(ExternalNumberSequence).StartsAt(1).IncrementsBy(1);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SupportDeskDbContext).Assembly);
        ApplyAccessFilters(modelBuilder);
        SeedData.Apply(modelBuilder);
    }

    // Core rules 2, 3, 5 and 8: one central visibility rule that no endpoint can skip.
    private void ApplyAccessFilters(ModelBuilder modelBuilder)
    {
        // Requester: own tickets. Specialist: assigned to them or to one of their groups. Admin: all.
        modelBuilder.Entity<Ticket>().HasQueryFilter(t =>
            CurrentUserIsAdmin
            || t.RequesterId == CurrentUserId
            || (CurrentUserIsSpecialist
                && (t.AssignedUserId == CurrentUserId
                    || (t.AssignedGroupId != null && CurrentUserGroupIds.Contains(t.AssignedGroupId.Value)))));

        // Child rows follow the ticket's visibility.
        modelBuilder.Entity<TroubleshootingStep>().HasQueryFilter(x => Tickets.Any(t => t.Id == x.TicketId));
        modelBuilder.Entity<Attachment>().HasQueryFilter(x => Tickets.Any(t => t.Id == x.TicketId));
        modelBuilder.Entity<Resolution>().HasQueryFilter(x => Tickets.Any(t => t.Id == x.TicketId));
        modelBuilder.Entity<TicketHistory>().HasQueryFilter(x => Tickets.Any(t => t.Id == x.TicketId));

        // Internal notes and the agent's runs (its inputs and reasoning) are never returned to requesters.
        modelBuilder.Entity<TicketComment>().HasQueryFilter(x =>
            Tickets.Any(t => t.Id == x.TicketId) && (!x.IsInternal || CurrentUserIsStaff));
        modelBuilder.Entity<AgentRun>().HasQueryFilter(x =>
            CurrentUserIsStaff && Tickets.Any(t => t.Id == x.TicketId));

        modelBuilder.Entity<Notification>().HasQueryFilter(x => CurrentUserIsAdmin || x.UserId == CurrentUserId);

        // Only admins read the audit log. Writes are unaffected by filters.
        modelBuilder.Entity<AuditLog>().HasQueryFilter(x => CurrentUserIsAdmin);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Store enums as readable text.
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(32);
    }
}
