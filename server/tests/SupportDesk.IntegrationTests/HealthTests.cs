namespace SupportDesk.IntegrationTests;

[Collection(ApiCollection.Name)]
public class HealthTests(ApiFactory factory)
{
    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
