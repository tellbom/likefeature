-- news_view_events.sql
-- Insert-only news view event history.

CREATE TABLE IF NOT EXISTS news_view_events
(
    event_id        String,
    news_id         String,
    user_id         String,
    occurred_at_utc DateTime,
    source          String DEFAULT 'api'
)
ENGINE = MergeTree()
ORDER BY (news_id, occurred_at_utc, event_id);
