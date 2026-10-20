namespace SupportDesk.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string EntraObjectId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Origin Origin { get; set; }
    public bool IsActive { get; set; } = true;

    public List<UserRole> Roles { get; set; } = [];
    public List<GroupMember> Memberships { get; set; } = [];
}

public class UserRole
{
    public Guid UserId { get; set; }
    public RoleName Role { get; set; }
}

public class SpecialistGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Receives tickets whose category has no default group yet.
    public bool IsTriage { get; set; }

    public List<GroupMember> Members { get; set; } = [];
}

public class GroupMember
{
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public bool IsGroupAdmin { get; set; }
}
