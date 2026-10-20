using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstractions;
using SupportDesk.Domain;

namespace SupportDesk.Api.Controllers;

public record MeDto(Guid Id, string DisplayName, string Email, Origin Origin, IReadOnlyList<RoleName> Roles, IReadOnlyList<GroupDto> Groups);

public record GroupDto(Guid Id, string Name);

[ApiController]
[Route("api/me")]
public class MeController(IApplicationDbContext db, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<MeDto> Get(CancellationToken ct)
    {
        var user = await db.Users.FirstAsync(u => u.Id == currentUser.UserId, ct);
        var groupIds = currentUser.GroupIds;
        var groups = await db.SpecialistGroups
            .Where(g => groupIds.Contains(g.Id))
            .OrderBy(g => g.Name)
            .Select(g => new GroupDto(g.Id, g.Name))
            .ToListAsync(ct);

        return new MeDto(user.Id, user.DisplayName, user.Email, user.Origin, currentUser.Roles.Order().ToList(), groups);
    }
}
