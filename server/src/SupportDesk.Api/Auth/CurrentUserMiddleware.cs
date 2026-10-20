using Microsoft.EntityFrameworkCore;
using SupportDesk.Infrastructure.Persistence;

namespace SupportDesk.Api.Auth;

// Maps the token's identity (the Entra "oid" claim, or the dev-login equivalent) to a user row,
// and loads roles and groups from the database. Roles are never taken from the token.
public class CurrentUserMiddleware(RequestDelegate next)
{
    public const string ObjectIdClaim = "oid";

    public async Task InvokeAsync(HttpContext context, SupportDeskDbContext db, RequestCurrentUser currentUser)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var objectId = context.User.FindFirst(ObjectIdClaim)?.Value;
            var user = objectId is null
                ? null
                : await db.Users
                    .AsNoTracking()
                    .Include(u => u.Roles)
                    .Include(u => u.Memberships)
                    .FirstOrDefaultAsync(u => u.EntraObjectId == objectId && u.IsActive, context.RequestAborted);

            if (user is null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            currentUser.Set(
                user.Id,
                user.Origin,
                user.Roles.Select(r => r.Role).ToHashSet(),
                user.Memberships.Select(m => m.GroupId).ToList());
        }

        await next(context);
    }
}
