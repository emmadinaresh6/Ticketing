using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Tickets;
using SupportDesk.Domain;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController(TicketService tickets) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<TicketSummaryDto>> List(
        [FromQuery] TicketScope scope, [FromQuery] TicketStatus? status, CancellationToken ct) =>
        tickets.ListAsync(scope, status, ct);

    // Tickets the caller cannot see return 404, not 403, so their existence is not revealed.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketDetailDto>> Get(Guid id, CancellationToken ct) =>
        await tickets.GetAsync(id, ct) is { } ticket ? ticket : NotFound();

    [HttpPost]
    public async Task<ActionResult<TicketSummaryDto>> Create(CreateTicketRequest request, CancellationToken ct)
    {
        var created = await tickets.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(Guid id, AddCommentRequest request, CancellationToken ct) =>
        await tickets.AddCommentAsync(id, request, ct) is { } comment
            ? StatusCode(StatusCodes.Status201Created, comment)
            : NotFound();
}
