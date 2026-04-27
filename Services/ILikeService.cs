using likefeature.Models;

namespace likefeature.Services;

public interface ILikeService
{
    Task<LikeStatus> ToggleAsync(string userId, string newsId);
    Task<bool> GetStatusAsync(string userId, string newsId);
    Task<long> GetCountAsync(string newsId);
}
