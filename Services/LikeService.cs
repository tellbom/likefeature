using likefeature.Models;

namespace likefeature.Services;

public class LikeService : ILikeService
{
    private readonly IRedisLikeStateStore _redis;
    private readonly ILikeSyncRetryQueue _retryQueue;
    private readonly ILogger<LikeService> _logger;

    public LikeService(
        IRedisLikeStateStore redis,
        ILikeSyncRetryQueue retryQueue,
        ILogger<LikeService> logger)
    {
        _redis      = redis;
        _retryQueue = retryQueue;
        _logger     = logger;
    }

    public async Task<LikeStatus> ToggleAsync(string userId, string newsId)
    {
        // 1. Redis Lua 原子 toggle — 成功即可返回给调用方
        var redisResult = await _redis.ToggleAsync(userId, newsId);

        _logger.LogInformation(
            "Redis toggle success: {EventType} newsId={NewsId} userId={UserId} count={Count}",
            redisResult.EventType, newsId, userId, redisResult.LikeCount);

        var likeEvent = new LikeEvent
        {
            NewsId        = newsId,
            UserId        = userId,
            EventType     = redisResult.EventType,
            OccurredAtUtc = DateTime.UtcNow
        };

        // 2. 同步入队 ClickHouse 写入任务（R2 修复：不再 fire-and-forget scoped 服务）
        await _retryQueue.EnqueueAsync(new RetryMessage
        {
            Event            = likeEvent,
            Target           = RetryTarget.ClickHouse,
            AttemptCount     = 0,
            NextAttemptAtUtc = DateTime.UtcNow,
            LikeCount        = redisResult.LikeCount
        });

        // 3. 同步入队 ES 投影任务
        await _retryQueue.EnqueueAsync(new RetryMessage
        {
            Event            = likeEvent,
            Target           = RetryTarget.Elasticsearch,
            AttemptCount     = 0,
            NextAttemptAtUtc = DateTime.UtcNow,
            LikeCount        = redisResult.LikeCount
        });

        return new LikeStatus
        {
            NewsId    = newsId,
            Liked     = redisResult.Liked,
            LikeCount = redisResult.LikeCount,
            EventType = redisResult.EventType
        };
    }

    public async Task<bool> GetStatusAsync(string userId, string newsId)
    {
        return await _redis.IsLikedAsync(userId, newsId);
    }

    public async Task<long> GetCountAsync(string newsId)
    {
        return await _redis.GetCountAsync(newsId);
    }
}
