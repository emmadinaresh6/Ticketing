using System.Net;
using System.Net.Http.Json;
using SupportDesk.Application.Categories;

namespace SupportDesk.IntegrationTests;

[Collection(ApiCollection.Name)]
public class CategoryTests(ApiFactory factory)
{
    [Fact]
    public async Task Duplicate_name_returns_the_existing_category()
    {
        var client = await factory.ClientForAsync("dev-customer");

        var response = await client.PostAsJsonAsync("/api/categories", new { name = "  network and VPN " });
        var result = await response.Content.ReadFromJsonAsync<AddCategoryResult>(ApiFactory.Json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(result!.Created);
        Assert.Equal("Network & VPN", result.Category.Name);
    }

    [Fact]
    public async Task Any_user_can_add_a_new_category_which_starts_without_routing()
    {
        var client = await factory.ClientForAsync("dev-customer");

        var response = await client.PostAsJsonAsync("/api/categories", new { name = "Printing &  scanning" });
        var result = await response.Content.ReadFromJsonAsync<AddCategoryResult>(ApiFactory.Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(result!.Created);
        Assert.Equal("Printing & scanning", result.Category.Name);
        Assert.False(result.Category.HasRouting);
    }
}
