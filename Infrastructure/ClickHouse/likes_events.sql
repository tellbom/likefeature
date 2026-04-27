-- likes_events.sql
-- 运行一次即可，insert-only，不允许 UPDATE / DELETE。

CREATE TABLE IF NOT EXISTS likes_events
(
    event_id       String,
    news_id        String,
    user_id        String,
    event_type     Enum8('Liked' = 1, 'Unliked' = 2),
    occurred_at_utc DateTime,
    source         String DEFAULT 'api'
)
ENGINE = MergeTree()
ORDER BY (news_id, occurred_at_utc, event_id);
