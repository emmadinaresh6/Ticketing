using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportDesk.Domain.Entities;

namespace SupportDesk.Infrastructure.Persistence.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.Property(x => x.EntraObjectId).HasMaxLength(64);
        b.HasIndex(x => x.EntraObjectId).IsUnique();
        b.Property(x => x.DisplayName).HasMaxLength(200);
        b.Property(x => x.Email).HasMaxLength(320);
        b.HasIndex(x => x.Email).IsUnique();
        b.HasMany(x => x.Roles).WithOne().HasForeignKey(x => x.UserId);
        b.HasMany(x => x.Memberships).WithOne().HasForeignKey(x => x.UserId);
    }
}

internal class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> b) => b.HasKey(x => new { x.UserId, x.Role });
}

internal class SpecialistGroupConfiguration : IEntityTypeConfiguration<SpecialistGroup>
{
    public void Configure(EntityTypeBuilder<SpecialistGroup> b)
    {
        b.Property(x => x.Name).HasMaxLength(100);
        b.HasIndex(x => x.Name).IsUnique();
        b.HasMany(x => x.Members).WithOne().HasForeignKey(x => x.GroupId);
    }
}

internal class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> b) => b.HasKey(x => new { x.GroupId, x.UserId });
}

internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.Property(x => x.Name).HasMaxLength(100);
        b.Property(x => x.NormalizedName).HasMaxLength(100);
        b.HasIndex(x => x.NormalizedName).IsUnique();
        b.HasOne<SpecialistGroup>().WithMany().HasForeignKey(x => x.DefaultGroupId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);
    }
}
