using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstractions;
using SupportDesk.Domain;

namespace SupportDesk.Infrastructure.Persistence;

// INT-000123 / EXT-000123 from a SQL Server sequence per origin. Sequences never hand out the same value twice.
internal class TicketNumberGenerator(SupportDeskDbContext db) : ITicketNumberGenerator
{
    public async Task<string> NextAsync(Origin origin, CancellationToken cancellationToken)
    {
        var (sql, prefix) = origin switch
        {
            Origin.Internal => ("SELECT NEXT VALUE FOR [" + SupportDeskDbContext.InternalNumberSequence + "] AS [Value]", "INT"),
            Origin.External => ("SELECT NEXT VALUE FOR [" + SupportDeskDbContext.ExternalNumberSequence + "] AS [Value]", "EXT"),
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };

        // ToListAsync runs the SQL as written. Composing (e.g. SingleAsync) would wrap it in a
        // subquery, where SQL Server does not allow NEXT VALUE FOR.
        var values = await db.Database.SqlQueryRaw<long>(sql).ToListAsync(cancellationToken);
        var next = values.Single();

        return $"{prefix}-{next:D6}";
    }
}
