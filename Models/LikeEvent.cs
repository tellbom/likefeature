namespace likefeature.Models;

public class LikeEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string NewsId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public LikeEventType EventType { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = "api";
}
