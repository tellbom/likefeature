using likefeature.Models;

namespace likefeature.Services;

public interface ILikeSyncRetryQueue
{
    /// <summary>
    /// 将失败的下游写入任务加入重试队列。
    /// </summary>
    ValueTask EnqueueAsync(RetryMessage message);

    /// <summary>
    /// 从队列取出下一条待重试消息，供后台 Worker 消费。
    /// </summary>
    ValueTask<RetryMessage> DequeueAsync(CancellationToken cancellationToken);
}
