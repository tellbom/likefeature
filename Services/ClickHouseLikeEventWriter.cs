using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using likefeature.Models;

namespace likefeature.Services;

public class ClickHouseLikeEventWriter : IClickHouseLikeEventWriter
{
    private readonly ClickHouseConnection _connection;
    private readonly ILogger<ClickHouseLikeEventWriter> _logger;

    public ClickHouseLikeEventWriter(
        ClickHouseConnection connection,
        ILogger<ClickHouseLikeEventWriter> logger)
    {
        _connection = connection;
        _logger     = logger;
    }

    public async Task AppendAsync(LikeEvent likeEvent)
    {
        const string sql = @"
            INSERT INTO likes_events
                (event_id, news_id, user_id, event_type, occurred_at_utc, source)
            VALUES
                ({p0}, {p1}, {p2}, {p3}, {p4}, {p5})";

        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "p0", Value = likeEvent.EventId });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "p1", Value = likeEvent.NewsId });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "p2", Value = likeEvent.UserId });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "p3", Value = likeEvent.EventType.ToString() });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "p4", Value = likeEvent.OccurredAtUtc });
        cmd.Parameters.Add(new ClickHouseDbParameter { ParameterName = "p5", Value = likeEvent.Source });

        await cmd.ExecuteNonQueryAsync();

        _logger.LogInformation(
            "ClickHouse event appended: {EventType} newsId={NewsId} userId={UserId}",
            likeEvent.EventType, likeEvent.NewsId, likeEvent.UserId);
    }
}
