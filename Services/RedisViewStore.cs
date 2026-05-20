using StackExchange.Redis;

namespace likefeature.Services;

public class RedisViewStore : IRedisViewStore
{
    private readonly IDatabase _db;

    private static readonly LuaScript ViewScript = LuaScript.Prepare(@"
        local isMember = redis.call('SISMEMBER', @usersKey, @userId)
        if isMember == 0 then
            redis.call('SADD', @usersKey, @userId)
            local count = redis.call('INCR', @countKey)
            return { 1, count }
        else
            local count = redis.call('GET', @countKey)
            return { 0, count or 0 }
        end
    ");

    public RedisViewStore(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<RedisViewResult> RecordAsync(string userId, string newsId)
    {
        var raw = await _db.ScriptEvaluateAsync(ViewScript, new
        {
            usersKey = (RedisKey) UsersKey(newsId),
            countKey = (RedisKey) CountKey(newsId),
            userId = (RedisValue) userId
        });

        if (raw.IsNull)
            throw new InvalidOperationException(
                "Redis view script returned null.");

        var result = (RedisResult[]?) raw;
        if (result is null || result.Length < 2)
            throw new InvalidOperationException(
                $"Redis view script returned {result?.Length ?? 0} elements, expected 2.");

        return new RedisViewResult
        {
            Recorded = (long) result[0] == 1,
            ViewCount = (long) result[1]
        };
    }

    public async Task<long> GetCountAsync(string newsId)
    {
        var value = await _db.StringGetAsync(CountKey(newsId));
        return value.HasValue ? (long) value : 0;
    }

    private static string UsersKey(string newsId) => $"views:users:{newsId}";
    private static string CountKey(string newsId) => $"views:count:{newsId}";
}
