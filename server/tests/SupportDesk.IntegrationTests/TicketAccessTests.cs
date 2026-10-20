using System.Net;
using System.Net.Http.Json;
using SupportDesk.Application.Tickets;
using SupportDesk.Domain;
using SupportDesk.Infrastructure.Persistence;

namespace SupportDesk.IntegrationTests;

// Core rules 1, 2, 3, 5 and 8 from CONTEXT.md, exercised through the real API.
[Collection(ApiCollection.Name)]
public class TicketAccessTests(ApiFactory factory)
{
    private const string Requester = "dev-requester";
    private const string Customer = "dev-customer";
    private const string Specialist = "dev-specialist";
    private const string Admin = "dev-admin";

    private static readonly Guid NetworkCategoryId = new("30000000-0000-0000-0000-000000000001");

    [Fact]
    public async Task Internal_user_gets_INT_number_and_customer_gets_EXT_number()
    {
        var internalTicket = await CreateTicketAsync(Requester);
        var externalTicket = await CreateTicketAsync(Customer);

        Assert.Matches(@"^INT-\d{6}$", internalTicket.Number);
        Assert.Equal(Origin.Internal, internalTicket.Origin);
        Assert.Matches(@"^EXT-\d{6}$", externalTicket.Number);
        Assert.Equal(Origin.External, externalTicket.Origin);
    }

    [Fact]
    public async Task Requester_cannot_see_another_requesters_ticket()
    {
        var ticket = await CreateTicketAsync(Requester);
        var customer = await factory.ClientForAsync(Customer);

        var get = await customer.GetAsync($"/api/tickets/{ticket.Id}");
        var list = await customer.GetFromJsonAsync<List<TicketSummaryDto>>("/api/tickets", ApiFactory.Json);

        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        Assert.DoesNotContain(list!, t => t.Id == ticket.Id);
    }

    [Fact]
    public async Task Specialist_sees_ticket_only_once_assigned_to_their_group()
    {
        var ticket = await CreateTicketAsync(Requester);
        var specialist = await factory.ClientForAsync(Specialist);

        Assert.Equal(HttpStatusCode.NotFound, (await specialist.GetAsync($"/api/tickets/{ticket.Id}")).StatusCode);

        await factory.AssignAsync(ticket.Id, SeedData.NetworkGroupId);

        Assert.Equal(HttpStatusCode.OK, (await specialist.GetAsync($"/api/tickets/{ticket.Id}")).StatusCode);
    }

    [Fact]
    public async Task Specialist_cannot_see_ticket_assigned_to_a_group_they_are_not_in()
    {
        var ticket = await CreateTicketAsync(Requester);
        await factory.AssignAsync(ticket.Id, SeedData.EndpointGroupId);
        var specialist = await factory.ClientForAsync(Specialist);

        var response = await specialist.GetAsync($"/api/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Specialist_sees_ticket_assigned_to_them_personally()
    {
        var ticket = await CreateTicketAsync(Requester);
        await factory.AssignAsync(ticket.Id, SeedData.EndpointGroupId, SeedData.SpecialistUserId);
        var specialist = await factory.ClientForAsync(Specialist);

        var response = await specialist.GetAsync($"/api/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Admin_sees_every_ticket()
    {
        var internalTicket = await CreateTicketAsync(Requester);
        var externalTicket = await CreateTicketAsync(Customer);
        var admin = await factory.ClientForAsync(Admin);

        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync($"/api/tickets/{internalTicket.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync($"/api/tickets/{externalTicket.Id}")).StatusCode);
    }

    [Fact]
    public async Task Internal_notes_are_never_returned_to_the_requester()
    {
        var ticket = await CreateTicketAsync(Requester);
        var admin = await factory.ClientForAsync(Admin);
        (await admin.PostAsJsonAsync($"/api/tickets/{ticket.Id}/comments", new { body = "Checking lockout logs", isInternal = true })).EnsureSuccessStatusCode();
        (await admin.PostAsJsonAsync($"/api/tickets/{ticket.Id}/comments", new { body = "Could you restart and retry?", isInternal = false })).EnsureSuccessStatusCode();

        var requester = await factory.ClientForAsync(Requester);
        var asRequester = await requester.GetFromJsonAsync<TicketDetailDto>($"/api/tickets/{ticket.Id}", ApiFactory.Json);
        var asAdmin = await admin.GetFromJsonAsync<TicketDetailDto>($"/api/tickets/{ticket.Id}", ApiFactory.Json);

        Assert.Single(asRequester!.Comments);
        Assert.False(asRequester.Comments[0].IsInternal);
        Assert.Equal(2, asAdmin!.Comments.Count);
    }

    [Fact]
    public async Task Requester_cannot_add_an_internal_note()
    {
        var ticket = await CreateTicketAsync(Requester);
        var requester = await factory.ClientForAsync(Requester);

        var response = await requester.PostAsJsonAsync($"/api/tickets/{ticket.Id}/comments", new { body = "hidden?", isInternal = true });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Viewing_a_ticket_is_audited()
    {
        var ticket = await CreateTicketAsync(Requester);
        var requester = await factory.ClientForAsync(Requester);

        await requester.GetAsync($"/api/tickets/{ticket.Id}");
        await requester.GetAsync($"/api/tickets/{ticket.Id}");

        Assert.Equal(2, await factory.CountAuditAsync(ticket.Id, "Viewed"));
    }

    [Fact]
    public async Task Mine_scope_excludes_group_tickets_and_queue_scope_excludes_own_tickets()
    {
        var specialist = await factory.ClientForAsync(Specialist);
        var own = await CreateTicketAsync(Specialist);
        var groupTicket = await CreateTicketAsync(Requester);
        await factory.AssignAsync(groupTicket.Id, SeedData.NetworkGroupId);

        var mine = await specialist.GetFromJsonAsync<List<TicketSummaryDto>>("/api/tickets?scope=Mine", ApiFactory.Json);
        var queue = await specialist.GetFromJsonAsync<List<TicketSummaryDto>>("/api/tickets?scope=Queue", ApiFactory.Json);

        Assert.Contains(mine!, t => t.Id == own.Id);
        Assert.DoesNotContain(mine!, t => t.Id == groupTicket.Id);
        Assert.Contains(queue!, t => t.Id == groupTicket.Id);
        Assert.DoesNotContain(queue!, t => t.Id == own.Id);
    }

    [Fact]
    public async Task Calls_without_sign_in_are_rejected()
    {
        var anonymous = factory.CreateClient();

        var response = await anonymous.GetAsync("/api/tickets");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<TicketSummaryDto> CreateTicketAsync(string devUser)
    {
        var client = await factory.ClientForAsync(devUser);
        var response = await client.PostAsJsonAsync("/api/tickets", new
        {
            title = "VPN disconnects every few minutes",
            description = "Drops roughly every five minutes on home Wi-Fi.",
            categoryId = NetworkCategoryId,
            priority = "Medium",
            severity = "Moderate"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<TicketSummaryDto>(ApiFactory.Json))!;
    }
}
