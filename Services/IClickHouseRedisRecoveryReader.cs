using likefeature.Models;

namespace likefeature.Services;

public interface IClickHouseRedisRecoveryReader
{
    Task<IReadOnlyCollection<RecoveredUsersForNews>> ReadCurrentLikedUsersAsync();

    Task<IReadOnlyCollection<RecoveredUsersForNews>> ReadViewedUsersAsync();
}
