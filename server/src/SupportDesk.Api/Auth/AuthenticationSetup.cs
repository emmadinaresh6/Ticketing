using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SupportDesk.Application.Abstractions;

namespace SupportDesk.Api.Auth;

public static class AuthenticationSetup
{
    public static IServiceCollection AddSupportDeskAuth(this IServiceCollection services, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            // Entra ID (staff) and Entra External ID (customers) are configured here later.
            throw new InvalidOperationException("Only dev login is implemented. Run in the Development environment.");
        }

        var issuer = new DevTokenIssuer();
        services.AddSingleton(issuer);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = DevTokenIssuer.Issuer,
                    ValidAudience = DevTokenIssuer.Audience,
                    IssuerSigningKey = issuer.SigningKey,
                    NameClaimType = "name"
                };
            });

        // Every endpoint requires a signed-in user unless it opts out with AllowAnonymous.
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        services.AddScoped<RequestCurrentUser>();
        services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<RequestCurrentUser>());

        return services;
    }
}
