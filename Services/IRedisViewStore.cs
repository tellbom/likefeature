namespace likefeature.Services;

public class RedisViewResult
{
    public bool Recorded { get; set; }
    public long ViewCount { get; set; }
}

public interface IRedisViewStore
{
    Task<RedisViewResult> RecordAsync(string userId, string newsId);

    Task<long> GetCountAsync(string newsId);
}
