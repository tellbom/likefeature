using likefeature.Models;

namespace likefeature.Services;

public class RedisRecoveryService : IRedisRecoveryService
{
    private readonly IClickHouseRedisRecoveryReader _reader;
    private readonly IRedisLikeStateStore _likes;
    private readonly IRedisViewStore _views;
    private readonly ILogger<RedisRecoveryService> _logger;

    public RedisRecoveryService(
        IClickHouseRedisRecoveryReader reader,
        IRedisLikeStateStore likes,
        IRedisViewStore views,
        ILogger<RedisRecoveryService> logger)
    {
        _reader = reader;
        _likes = likes;
        _views = views;
        _logger = logger;
    }

    public async Task<RedisRecoveryResponse> RecoverAsync(RedisRecoveryRequest request)
    {
        var response = new RedisRecoveryResponse
        {
            Success = true,
            Likes = new RedisRecoveryItemResult { Requested = request.RecoverLikes },
            Views = new RedisRecoveryItemResult { Requested = request.RecoverViews }
        };

        if (request.RecoverLikes)
            response.Likes = await RecoverLikesAsync();

        if (request.RecoverViews)
            response.Views = await RecoverViewsAsync();

        return response;
    }

    private async Task<RedisRecoveryItemResult> RecoverLikesAsync()
    {
        var items = await _reader.ReadCurrentLikedUsersAsync();
        var relationCount = 0L;

        foreach (var item in items)
        {
            await _likes.RestoreLikedUsersAsync(item.NewsId, item.UserIds);
            relationCount += item.UserIds.Count;
        }

        _logger.LogWarning(
            "Redis likes recovered from ClickHouse: newsCount={NewsCount} userRelationCount={RelationCount}",
            items.Count,
            relationCount);

        return new RedisRecoveryItemResult
        {
            Requested = true,
            NewsCount = items.Count,
            UserRelationCount = relationCount
        };
    }

    private async Task<RedisRecoveryItemResult> RecoverViewsAsync()
    {
        var items = await _reader.ReadViewedUsersAsync();
        var relationCount = 0L;

        foreach (var item in items)
        {
            await _views.RestoreViewedUsersAsync(item.NewsId, item.UserIds);
            relationCount += item.UserIds.Count;
        }

        _logger.LogWarning(
            "Redis views recovered from ClickHouse: newsCount={NewsCount} userRelationCount={RelationCount}",
            items.Count,
            relationCount);

        return new RedisRecoveryItemResult
        {
            Requested = true,
            NewsCount = items.Count,
            UserRelationCount = relationCount
        };
    }
}
