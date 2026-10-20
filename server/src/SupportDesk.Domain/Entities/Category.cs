using System.Text.RegularExpressions;

namespace SupportDesk.Domain.Entities;

public partial class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Unique. Used to block duplicates such as "Network & VPN" vs "network and vpn".
    public string NormalizedName { get; set; } = string.Empty;

    // Null for user-added categories until an admin sets routing; tickets then go to the triage group.
    public Guid? DefaultGroupId { get; set; }

    public Guid? CreatedById { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public static string Normalize(string name)
    {
        var text = name.ToLowerInvariant().Replace("&", " and ");
        return NonAlphanumeric().Replace(text, " ").Trim();
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();
}
