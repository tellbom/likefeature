using likefeature.Models;
using StackExchange.Redis;

namespace likefeature.Services;

public class RedisLikeStateStore : IRedisLikeStateStore
{
    private readonly IDatabase _db;

    private static readonly LuaScript ToggleScript = LuaScript.Prepare(@"
        local isMember = redis.call('SISMEMBER', @usersKey, @userId)
        if isMember == 0 then
            redis.call('SADD', @usersKey, @userId)
            local count = redis.call('INCR', @countKey)
            return { 1, count }
        else
            redis.call('SREM', @usersKey, @userId)
            local count = redis.call('DECR', @countKey)
            if count < 0 then
                redis.call('SET', @countKey, 0)
                count = 0
            end
            return { 0, count }
        end
    ");

    public RedisLikeStateStore(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<RedisToggleResult> ToggleAsync(string userId, string newsId)
    {
        var raw = await _db.ScriptEvaluateAsync(ToggleScript, new
        {
            usersKey = (RedisKey)  UsersKey(newsId),
            countKey = (RedisKey)  CountKey(newsId),
            userId   = (RedisValue) userId
        });

        var result = (RedisResult[]?) raw;
        if (result is null || result.Length < 2)
            throw new InvalidOperationException(
                $"Redis toggle script returned {result?.Length ?? 0} elements, expected 2.");

        var liked     = (long) result[0] == 1;
        var likeCount = (long) result[1];

        return new RedisToggleResult
        {
            Liked     = liked,
            LikeCount = likeCount,
            EventType = liked ? LikeEventType.Liked : LikeEventType.Unliked
        };
    }

    public async Task<bool> IsLikedAsync(string userId, string newsId)
    {
        return await _db.SetContainsAsync(UsersKey(newsId), userId);
    }

    public async Task<long> GetCountAsync(string newsId)
    {
        var value = await _db.StringGetAsync(CountKey(newsId));
        return value.HasValue ? (long) value : 0;
    }

    public async Task RestoreLikedUsersAsync(string newsId, IReadOnlyCollection<string> userIds)
    {
        var usersKey = UsersKey(newsId);
        var countKey = CountKey(newsId);

        await _db.KeyDeleteAsync(usersKey);

        if (userIds.Count > 0)
        {
            var values = userIds.Select(userId => (RedisValue) userId).ToArray();
            await _db.SetAddAsync(usersKey, values);
        }

        await _db.StringSetAsync(countKey, userIds.Count);
    }

    private static string UsersKey(string newsId) => $"likes:users:{newsId}";
    private static string CountKey(string newsId)  => $"likes:count:{newsId}";
}
