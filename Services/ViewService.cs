using likefeature.Models;

namespace likefeature.Services;

public class ViewService : IViewService
{
    private readonly IRedisViewStore _redis;
    private readonly ILikeSyncRetryQueue _retryQueue;
    private readonly ILogger<ViewService> _logger;

    public ViewService(
        IRedisViewStore redis,
        ILikeSyncRetryQueue retryQueue,
        ILogger<ViewService> logger)
    {
        _redis = redis;
        _retryQueue = retryQueue;
        _logger = logger;
    }

    public async Task<(bool Recorded, long ViewCount)> RecordAsync(string userId, string newsId)
    {
        var result = await _redis.RecordAsync(userId, newsId);

        _logger.LogInformation(
            "View record: recorded={Recorded} newsId={NewsId} userId={UserId} viewCount={Count}",
            result.Recorded, newsId, userId, result.ViewCount);

        if (result.Recorded)
        {
            var viewEvent = new ViewEvent
            {
                NewsId = newsId,
                UserId = userId,
                OccurredAtUtc = DateTime.UtcNow
            };

            await _retryQueue.EnqueueAsync(new RetryMessage
            {
                ViewEvent = viewEvent,
                Target = RetryTarget.ClickHouse,
                AttemptCount = 0,
                NextAttemptAtUtc = DateTime.UtcNow
            });
        }

        return (result.Recorded, result.ViewCount);
    }

    public async Task<long> GetCountAsync(string newsId)
    {
        return await _redis.GetCountAsync(newsId);
    }
}
