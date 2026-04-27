using likefeature.Models;

namespace likefeature.Services;

/// <summary>
/// 后台补偿 Worker：从重试队列消费消息，执行 ClickHouse 追加和 ES upsert。
/// 失败时按指数退避重新入队，直到达到最大重试次数后记录 dead-letter 日志。
/// R2 修复：未到期消息立即重新入队，不 Delay 阻塞单消费者队列。
/// </summary>
public class LikeSyncRetryWorker : BackgroundService
{
    private readonly ILikeSyncRetryQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LikeSyncRetryWorker> _logger;
    private readonly int _maxRetryCount;

    public LikeSyncRetryWorker(
        ILikeSyncRetryQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<LikeSyncRetryWorker> logger,
        IConfiguration config)
    {
        _queue         = queue;
        _scopeFactory  = scopeFactory;
        _logger        = logger;
        _maxRetryCount = config.GetValue<int>("LikeSync:MaxRetryCount", 10);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LikeSyncRetryWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            RetryMessage message;
            try
            {
                message = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            // R2 修复：未到期直接重新入队，不 Delay 阻塞消费者
            if (message.NextAttemptAtUtc > DateTime.UtcNow)
            {
                await _queue.EnqueueAsync(message);
                // 短暂让出 CPU，防止未到期消息在空队列时形成忙轮询
                await Task.Delay(200, stoppingToken);
                continue;
            }

            await ProcessAsync(message, stoppingToken);
        }

        _logger.LogInformation("LikeSyncRetryWorker stopped.");
    }

    private async Task ProcessAsync(RetryMessage message, CancellationToken stoppingToken)
    {
        if (message.AttemptCount >= _maxRetryCount)
        {
            _logger.LogError(
                "Dead-letter: max retries reached. target={Target} newsId={NewsId} messageId={MessageId}",
                message.Target, message.Event.NewsId, message.MessageId);
            return;
        }

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            switch (message.Target)
            {
                case RetryTarget.ClickHouse:
                    var chWriter = scope.ServiceProvider
                        .GetRequiredService<IClickHouseLikeEventWriter>();
                    await chWriter.AppendAsync(message.Event);
                    break;

                case RetryTarget.Elasticsearch:
                    var esStore = scope.ServiceProvider
                        .GetRequiredService<IElasticsearchLikeQueryStore>();
                    // A1：优先从 Redis 读最新 count，降级用 RetryMessage 快照值
                    var redisStore = scope.ServiceProvider
                        .GetRequiredService<IRedisLikeStateStore>();
                    long latestCount;
                    try
                    {
                        latestCount = await redisStore.GetCountAsync(message.Event.NewsId);
                    }
                    catch
                    {
                        latestCount = message.LikeCount;
                    }
                    await esStore.UpsertAsync(message.Event.NewsId, latestCount);
                    break;
            }

            _logger.LogInformation(
                "Retry success: target={Target} newsId={NewsId} attempt={Attempt}",
                message.Target, message.Event.NewsId, message.AttemptCount + 1);
        }
        catch (Exception ex)
        {
            message.AttemptCount++;
            message.LastError        = ex.Message;
            var backoffSeconds       = Math.Min(5 * Math.Pow(2, message.AttemptCount - 1), 300);
            message.NextAttemptAtUtc = DateTime.UtcNow.AddSeconds(backoffSeconds);

            _logger.LogWarning(ex,
                "Retry failed, re-enqueuing: target={Target} newsId={NewsId} attempt={Attempt} nextIn={Backoff}s",
                message.Target, message.Event.NewsId, message.AttemptCount, backoffSeconds);

            await _queue.EnqueueAsync(message);
        }
    }
}
