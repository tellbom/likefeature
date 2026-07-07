using likefeature.Models;

namespace likefeature.Services;

public interface IRedisRecoveryService
{
    Task<RedisRecoveryResponse> RecoverAsync(RedisRecoveryRequest request);
}
