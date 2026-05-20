using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using likefeature.Models;

namespace likefeature.Services;

public class ClickHouseViewEventWriter : IClickHouseViewEventWriter
{
    private readonly ClickHouseConnection _connection;
    private readonly ILogger<ClickHouseViewEventWriter> _logger;

    public ClickHouseViewEventWriter(
        ClickHouseConnection connection,
        ILogger<ClickHouseViewEventWriter> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task AppendAsync(ViewEvent viewEvent)
    {
        const string sql = @"
            INSERT INTO news_view_events
                (event_id, news_id, user_id, occurred_at_utc, source)
            VALUES
                (@p0, @p1, @p2, @p3, @p4)";

        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "p0", Value = viewEvent.EventId });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "p1", Value = viewEvent.NewsId });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "p2", Value = viewEvent.UserId });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "p3", Value = viewEvent.OccurredAtUtc });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "p4", Value = viewEvent.Source });

        await cmd.ExecuteNonQueryAsync();

        _logger.LogInformation(
            "ClickHouse view event appended: newsId={NewsId} userId={UserId}",
            viewEvent.NewsId, viewEvent.UserId);
    }
}
