using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SupportDesk.Infrastructure.Persistence;

namespace SupportDesk.Api.Auth;

// Development-only sign-in with the seeded users. Swapped for Entra ID / Entra External ID later:
// only AuthenticationSetup changes, because everything downstream reads the "oid" claim.
public class DevTokenIssuer
{
    public const string Issuer = "supportdesk-dev";
    public const string Audience = "supportdesk-api";

    // New random key per process: no secret in code or config. Tokens stop working when the API restarts.
    public SymmetricSecurityKey SigningKey { get; } = new(RandomNumberGenerator.GetBytes(32));

    public string Issue(string objectId, string displayName, TimeSpan lifetime)
    {
        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            Subject = new ClaimsIdentity([
                new Claim(CurrentUserMiddleware.ObjectIdClaim, objectId),
                new Claim("name", displayName)
            ]),
            Expires = DateTime.UtcNow.Add(lifetime),
            SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256)
        });
    }
}

public record DevLoginRequest([Required] string User);

public record DevLoginResponse(string AccessToken, DateTimeOffset ExpiresAt);

public record DevUserDto(string Key, string DisplayName, string Origin, IReadOnlyList<string> Roles);

public static class DevAuthEndpoints
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(8);

    public static void MapDevAuth(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dev").AllowAnonymous().WithTags("Dev login");

        group.MapGet("/users", async (SupportDeskDbContext db, CancellationToken ct) =>
            await db.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.DisplayName)
                .Select(u => new DevUserDto(
                    u.EntraObjectId,
                    u.DisplayName,
                    u.Origin.ToString(),
                    u.Roles.Select(r => r.Role.ToString()).ToList()))
                .ToListAsync(ct));

        group.MapPost("/login", async (DevLoginRequest request, SupportDeskDbContext db, DevTokenIssuer issuer, CancellationToken ct) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.EntraObjectId == request.User && u.IsActive, ct);
            if (user is null)
            {
                return Results.Problem("Unknown dev user.", statusCode: StatusCodes.Status400BadRequest);
            }

            var token = issuer.Issue(user.EntraObjectId, user.DisplayName, TokenLifetime);
            return Results.Ok(new DevLoginResponse(token, DateTimeOffset.UtcNow.Add(TokenLifetime)));
        });
    }
}
