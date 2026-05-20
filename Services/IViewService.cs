namespace likefeature.Services;

public interface IViewService
{
    Task<(bool Recorded, long ViewCount)> RecordAsync(string userId, string newsId);

    Task<long> GetCountAsync(string newsId);
}
