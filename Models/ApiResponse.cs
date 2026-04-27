namespace likefeature.Models;

public class ToggleLikeResponse
{
    public bool Success { get; set; }
    public string NewsId { get; set; } = string.Empty;
    public bool Liked { get; set; }
    public long LikeCount { get; set; }
    public string EventType { get; set; } = string.Empty;
}

public class LikeStatusResponse
{
    public bool Success { get; set; }
    public string NewsId { get; set; } = string.Empty;
    public bool Liked { get; set; }
}

public class LikeCountResponse
{
    public bool Success { get; set; }
    public string NewsId { get; set; } = string.Empty;
    public long LikeCount { get; set; }
}
