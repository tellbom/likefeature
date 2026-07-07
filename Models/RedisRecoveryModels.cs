namespace likefeature.Models;

public class RedisRecoveryRequest
{
    public bool RecoverLikes { get; set; } = true;
    public bool RecoverViews { get; set; } = true;
}

public class RedisRecoveryResponse
{
    public bool Success { get; set; }
    public RedisRecoveryItemResult Likes { get; set; } = new();
    public RedisRecoveryItemResult Views { get; set; } = new();
}

public class RedisRecoveryItemResult
{
    public bool Requested { get; set; }
    public int NewsCount { get; set; }
    public long UserRelationCount { get; set; }
}

public class RecoveredUsersForNews
{
    public string NewsId { get; set; } = string.Empty;
    public IReadOnlyCollection<string> UserIds { get; set; } = Array.Empty<string>();
}
