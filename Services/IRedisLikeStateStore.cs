using likefeature.Models;

namespace likefeature.Services;

public class RedisToggleResult
{
    public bool Liked { get; set; }
    public long LikeCount { get; set; }
    public LikeEventType EventType { get; set; }
}

public interface IRedisLikeStateStore
{
    /// <summary>
    /// 原子执行 toggle：用 Lua 脚本完成成员判断、集合写入、计数更新。
    /// </summary>
    Task<RedisToggleResult> ToggleAsync(string userId, string newsId);

    /// <summary>
    /// 查询指定用户是否已点赞。
    /// </summary>
    Task<bool> IsLikedAsync(string userId, string newsId);

    /// <summary>
    /// 查询新闻当前点赞数。
    /// </summary>
    Task<long> GetCountAsync(string newsId);

    Task RestoreLikedUsersAsync(string newsId, IReadOnlyCollection<string> userIds);
}
