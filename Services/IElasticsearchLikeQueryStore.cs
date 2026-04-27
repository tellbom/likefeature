using likefeature.Models;

namespace likefeature.Services;

public interface IElasticsearchLikeQueryStore
{
    /// <summary>
    /// 更新或插入 newsId 对应的点赞数投影文档。
    /// </summary>
    Task UpsertAsync(string newsId, long likeCount);
}
