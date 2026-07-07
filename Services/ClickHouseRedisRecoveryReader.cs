using ClickHouse.Client.ADO;
using likefeature.Models;

namespace likefeature.Services;

public class ClickHouseRedisRecoveryReader : IClickHouseRedisRecoveryReader
{
    private readonly ClickHouseConnection _connection;

    public ClickHouseRedisRecoveryReader(ClickHouseConnection connection)
    {
        _connection = connection;
    }

    public async Task<IReadOnlyCollection<RecoveredUsersForNews>> ReadCurrentLikedUsersAsync()
    {
        const string sql = @"
            SELECT news_id, user_id
            FROM
            (
                SELECT
                    news_id,
                    user_id,
                    argMax(toString(event_type), tuple(occurred_at_utc, event_id)) AS last_event_type
                FROM likes_events
                GROUP BY news_id, user_id
            )
            WHERE last_event_type = 'Liked'
            ORDER BY news_id, user_id";

        return await ReadGroupedUsersAsync(sql);
    }

    public async Task<IReadOnlyCollection<RecoveredUsersForNews>> ReadViewedUsersAsync()
    {
        const string sql = @"
            SELECT news_id, user_id
            FROM news_view_events
            GROUP BY news_id, user_id
            ORDER BY news_id, user_id";

        return await ReadGroupedUsersAsync(sql);
    }

    private async Task<IReadOnlyCollection<RecoveredUsersForNews>> ReadGroupedUsersAsync(string sql)
    {
        await EnsureOpenAsync();

        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        await using var reader = await cmd.ExecuteReaderAsync();
        var result = new List<RecoveredUsersForNews>();
        var currentNewsId = string.Empty;
        var currentUsers = new List<string>();

        while (await reader.ReadAsync())
        {
            var newsId = reader.GetString(0);
            var userId = reader.GetString(1);

            if (currentUsers.Count > 0 && newsId != currentNewsId)
            {
                result.Add(new RecoveredUsersForNews
                {
                    NewsId = currentNewsId,
                    UserIds = currentUsers.ToArray()
                });
                currentUsers.Clear();
            }

            currentNewsId = newsId;
            currentUsers.Add(userId);
        }

        if (currentUsers.Count > 0)
        {
            result.Add(new RecoveredUsersForNews
            {
                NewsId = currentNewsId,
                UserIds = currentUsers.ToArray()
            });
        }

        return result;
    }

    private async Task EnsureOpenAsync()
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync();
    }
}
