namespace likefeature.Models;

public class ViewEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string NewsId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = "api";
}

public class RecordViewResponse
{
    public bool Success { get; set; }
    public string NewsId { get; set; } = string.Empty;
    public bool Recorded { get; set; }
    public long ViewCount { get; set; }
}

public class ViewCountResponse
{
    public bool Success { get; set; }
    public string NewsId { get; set; } = string.Empty;
    public long ViewCount { get; set; }
}
