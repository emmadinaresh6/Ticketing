using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportDesk.Domain.Entities;

namespace SupportDesk.Infrastructure.Persistence.Configurations;

internal class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> b)
    {
        b.Property(x => x.Number).HasMaxLength(16);
        b.HasIndex(x => x.Number).IsUnique();
        b.Property(x => x.Title).HasMaxLength(200);
        b.Property(x => x.Description).HasMaxLength(8000);

        b.HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.RequesterId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.AssignedUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<SpecialistGroup>().WithMany().HasForeignKey(x => x.AssignedGroupId).OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Steps).WithOne().HasForeignKey(x => x.TicketId);
        b.HasMany(x => x.Comments).WithOne().HasForeignKey(x => x.TicketId);
        b.HasMany(x => x.Attachments).WithOne().HasForeignKey(x => x.TicketId);

        // Visibility rule lookups.
        b.HasIndex(x => x.RequesterId);
        b.HasIndex(x => x.AssignedGroupId);
        b.HasIndex(x => x.AssignedUserId);
    }
}

internal class TroubleshootingStepConfiguration : IEntityTypeConfiguration<TroubleshootingStep>
{
    public void Configure(EntityTypeBuilder<TroubleshootingStep> b)
    {
        b.Property(x => x.Instruction).HasMaxLength(2000);
        b.Property(x => x.OutcomeNote).HasMaxLength(2000);
        b.HasIndex(x => new { x.TicketId, x.Sequence }).IsUnique();
        b.HasOne<KbArticle>().WithMany().HasForeignKey(x => x.KbArticleId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> b)
    {
        b.Property(x => x.Body).HasMaxLength(8000);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> b)
    {
        b.Property(x => x.FileName).HasMaxLength(260);
        b.Property(x => x.BlobPath).HasMaxLength(500);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.UploadedById).OnDelete(DeleteBehavior.Restrict);
    }
}

internal class ResolutionConfiguration : IEntityTypeConfiguration<Resolution>
{
    public void Configure(EntityTypeBuilder<Resolution> b)
    {
        b.Property(x => x.Summary).HasMaxLength(4000);
        b.Property(x => x.RootCause).HasMaxLength(4000);
        b.HasOne<Ticket>().WithMany().HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.ReviewedById).OnDelete(DeleteBehavior.Restrict);
    }
}

internal class AgentRunConfiguration : IEntityTypeConfiguration<AgentRun>
{
    public void Configure(EntityTypeBuilder<AgentRun> b)
    {
        b.Property(x => x.PromptVersion).HasMaxLength(32);
        b.Property(x => x.ModelName).HasMaxLength(100);
        b.HasOne<Ticket>().WithMany().HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal class KbArticleConfiguration : IEntityTypeConfiguration<KbArticle>
{
    public void Configure(EntityTypeBuilder<KbArticle> b)
    {
        b.Property(x => x.Title).HasMaxLength(200);
        b.HasOne<Resolution>().WithMany().HasForeignKey(x => x.ResolutionId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal class SlaPolicyConfiguration : IEntityTypeConfiguration<SlaPolicy>
{
    public void Configure(EntityTypeBuilder<SlaPolicy> b) => b.HasIndex(x => new { x.Origin, x.Priority }).IsUnique();
}

internal class TicketHistoryConfiguration : IEntityTypeConfiguration<TicketHistory>
{
    public void Configure(EntityTypeBuilder<TicketHistory> b)
    {
        b.Property(x => x.Action).HasMaxLength(64);
        b.Property(x => x.OldValue).HasMaxLength(2000);
        b.Property(x => x.NewValue).HasMaxLength(2000);
        // No FK cascade: history must outlive any delete attempt.
        b.HasOne<Ticket>().WithMany().HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.TicketId);
    }
}

internal class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.Property(x => x.Action).HasMaxLength(64);
        b.HasIndex(x => x.TicketId);
        b.HasIndex(x => x.Timestamp);
    }
}

internal class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Ticket>().WithMany().HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
    }
}
