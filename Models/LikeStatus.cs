namespace likefeature.Models;

public class LikeStatus
{
    public string NewsId { get; set; } = string.Empty;
    public bool Liked { get; set; }
    public long LikeCount { get; set; }
    public LikeEventType EventType { get; set; }
}

public enum RetryTarget
{
    ClickHouse,
    Elasticsearch
}

public class RetryMessage
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public LikeEvent Event { get; set; } = new();
    public RetryTarget Target { get; set; }
    public int AttemptCount { get; set; }
    public DateTime NextAttemptAtUtc { get; set; } = DateTime.UtcNow;
    public string? LastError { get; set; }
    /// <summary>
    /// ES upsert 重试时使用。Worker 优先从 Redis 读最新值，
    /// 此字段作为 Redis 不可达时的降级备用值。
    /// </summary>
    public long LikeCount { get; set; }
}
