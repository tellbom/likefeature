using System.Threading.Channels;
using likefeature.Models;

namespace likefeature.Services;

/// <summary>
/// 基于 System.Threading.Channels 的内存重试队列。
/// Redis toggle 成功后，ClickHouse / ES 写入失败时将消息投入此队列，
/// 由后台 LikeSyncRetryWorker 消费重试。
/// </summary>
public class LikeSyncRetryQueue : ILikeSyncRetryQueue
{
    // 无界 channel：背压由 RetryWorker 的消费速度自然控制
    private readonly Channel<RetryMessage> _channel =
        Channel.CreateUnbounded<RetryMessage>(new UnboundedChannelOptions
        {
            SingleReader = true  // Worker 是唯一消费者
        });

    public ValueTask EnqueueAsync(RetryMessage message)
    {
        return _channel.Writer.WriteAsync(message);
    }

    public ValueTask<RetryMessage> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}
