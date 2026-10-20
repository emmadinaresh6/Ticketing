using SupportDesk.Domain;

namespace SupportDesk.Application.Abstractions;

// The signed-in caller, resolved once per request. Drives every access rule.
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    Origin Origin { get; }
    IReadOnlyCollection<RoleName> Roles { get; }
    IReadOnlyList<Guid> GroupIds { get; }

    bool IsAdmin => Roles.Contains(RoleName.Admin);
    bool IsSpecialist => Roles.Contains(RoleName.Specialist);
    bool IsStaff => IsAdmin || IsSpecialist;
}
