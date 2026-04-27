using likefeature.Models;

namespace likefeature.Services;

public interface IClickHouseLikeEventWriter
{
    /// <summary>
    /// 追加一条 Liked / Unliked 事件，insert-only，不允许更新或删除。
    /// </summary>
    Task AppendAsync(LikeEvent likeEvent);
}
