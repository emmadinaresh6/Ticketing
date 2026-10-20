using SupportDesk.Application.Abstractions;
using SupportDesk.Domain;

namespace SupportDesk.Api.Auth;

// Filled in by CurrentUserMiddleware once per request. Empty (sees nothing) until then.
public class RequestCurrentUser : ICurrentUser
{
    public bool IsAuthenticated => UserId is not null;
    public Guid? UserId { get; private set; }
    public Origin Origin { get; private set; }
    public IReadOnlyCollection<RoleName> Roles { get; private set; } = [];
    public IReadOnlyList<Guid> GroupIds { get; private set; } = [];

    public void Set(Guid userId, Origin origin, IReadOnlyCollection<RoleName> roles, IReadOnlyList<Guid> groupIds)
    {
        UserId = userId;
        Origin = origin;
        Roles = roles;
        GroupIds = groupIds;
    }
}
