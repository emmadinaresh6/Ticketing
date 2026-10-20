using Microsoft.EntityFrameworkCore;
using SupportDesk.Domain;
using SupportDesk.Domain.Entities;

namespace SupportDesk.Infrastructure.Persistence;

// Reference data plus the dev-login users. Fixed IDs so migrations stay stable.
public static class SeedData
{
    public static readonly Guid TriageGroupId = new("10000000-0000-0000-0000-000000000001");
    public static readonly Guid NetworkGroupId = new("10000000-0000-0000-0000-000000000002");
    public static readonly Guid IdentityGroupId = new("10000000-0000-0000-0000-000000000003");
    public static readonly Guid EndpointGroupId = new("10000000-0000-0000-0000-000000000004");

    public static readonly Guid RequesterUserId = new("20000000-0000-0000-0000-000000000001");
    public static readonly Guid CustomerUserId = new("20000000-0000-0000-0000-000000000002");
    public static readonly Guid SpecialistUserId = new("20000000-0000-0000-0000-000000000003");
    public static readonly Guid AdminUserId = new("20000000-0000-0000-0000-000000000004");

    private static readonly DateTimeOffset SeededAt = new(2026, 10, 19, 0, 0, 0, TimeSpan.Zero);

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SpecialistGroup>().HasData(
            new SpecialistGroup { Id = TriageGroupId, Name = "Service Desk Triage", IsTriage = true },
            new SpecialistGroup { Id = NetworkGroupId, Name = "Network Services" },
            new SpecialistGroup { Id = IdentityGroupId, Name = "Identity & Access" },
            new SpecialistGroup { Id = EndpointGroupId, Name = "Endpoint & Devices" });

        modelBuilder.Entity<Category>().HasData(
            NewCategory("30000000-0000-0000-0000-000000000001", "Network & VPN", NetworkGroupId),
            NewCategory("30000000-0000-0000-0000-000000000002", "Email & calendar", EndpointGroupId),
            NewCategory("30000000-0000-0000-0000-000000000003", "Hardware", EndpointGroupId),
            NewCategory("30000000-0000-0000-0000-000000000004", "Access & accounts", IdentityGroupId),
            NewCategory("30000000-0000-0000-0000-000000000005", "Other", TriageGroupId));

        modelBuilder.Entity<User>().HasData(
            NewUser(RequesterUserId, "dev-requester", "Priya Raman", "priya.raman@example.com", Origin.Internal),
            NewUser(CustomerUserId, "dev-customer", "Jordan Lee", "jordan.lee@example.org", Origin.External),
            NewUser(SpecialistUserId, "dev-specialist", "Sam Okafor", "sam.okafor@example.com", Origin.Internal),
            NewUser(AdminUserId, "dev-admin", "Alex Admin", "alex.admin@example.com", Origin.Internal));

        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { UserId = RequesterUserId, Role = RoleName.Requester },
            new UserRole { UserId = CustomerUserId, Role = RoleName.Requester },
            new UserRole { UserId = SpecialistUserId, Role = RoleName.Requester },
            new UserRole { UserId = SpecialistUserId, Role = RoleName.Specialist },
            new UserRole { UserId = AdminUserId, Role = RoleName.Requester },
            new UserRole { UserId = AdminUserId, Role = RoleName.Admin });

        modelBuilder.Entity<GroupMember>().HasData(
            new GroupMember { GroupId = NetworkGroupId, UserId = SpecialistUserId },
            new GroupMember { GroupId = IdentityGroupId, UserId = SpecialistUserId },
            new GroupMember { GroupId = TriageGroupId, UserId = AdminUserId, IsGroupAdmin = true });
    }

    private static Category NewCategory(string id, string name, Guid groupId) => new()
    {
        Id = new Guid(id),
        Name = name,
        NormalizedName = Category.Normalize(name),
        DefaultGroupId = groupId,
        CreatedAt = SeededAt
    };

    private static User NewUser(Guid id, string objectId, string name, string email, Origin origin) => new()
    {
        Id = id,
        EntraObjectId = objectId,
        DisplayName = name,
        Email = email,
        Origin = origin,
        IsActive = true
    };
}
