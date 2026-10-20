using SupportDesk.Domain.Entities;

namespace SupportDesk.UnitTests;

public class CategoryNormalizeTests
{
    [Theory]
    [InlineData("Network & VPN", "network and vpn")]
    [InlineData("network and vpn", "network and vpn")]
    [InlineData("  Network   &   VPN ", "network and vpn")]
    [InlineData("Email/Calendar!", "email calendar")]
    [InlineData("HARDWARE", "hardware")]
    public void Normalize_produces_comparable_key(string input, string expected)
    {
        Assert.Equal(expected, Category.Normalize(input));
    }

    [Fact]
    public void Ampersand_and_word_and_are_duplicates()
    {
        Assert.Equal(Category.Normalize("Access & accounts"), Category.Normalize("access and Accounts"));
    }
}
