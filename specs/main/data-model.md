# Data Model: 点赞功能微服务

## NewsLikeState

Current-state representation stored in Redis.

Fields:
- `newsId`: string, required, identifies one news article.
- `likedUserIds`: Redis set of user IDs that currently like the article.
- `likeCount`: integer, derived and maintained atomically with `likedUserIds`.

Validation rules:
- `newsId` must be present in toggle, status, and count requests.
- `userId` must be present for toggle and status requests.
- A user can appear at most once in `likedUserIds` for a single `newsId`.

State transitions:
- If `userId` is absent from `likedUserIds`, toggle adds it and increments `likeCount`; result event is `Liked`.
- If `userId` is present, toggle removes it and decrements `likeCount`; result event is `Unliked`.

## LikeEvent

Insert-only history representation stored in ClickHouse.

Fields:
- `eventId`: string/UUID, unique event identifier.
- `newsId`: string, required.
- `userId`: string, required.
- `eventType`: enum, `Liked` or `Unliked`.
- `occurredAtUtc`: timestamp, required.
- `source`: string, default `api`.

Validation rules:
- `eventType` must match the Redis Lua toggle result.
- Rows are append-only; no update or delete behavior is allowed for normal feature flow.

## LikeQueryDocument

Elasticsearch projection for query and aggregation.

Fields:
- `newsId`: string, document identifier.
- `likeCount`: integer, latest projected count.
- `updatedAtUtc`: timestamp of the latest projection update.

Consistency rules:
- ES is derived from Redis/ClickHouse sync and may lag.
- API count can use Redis for current count when serving the live endpoint.

## RetryMessage

Reliable compensation unit for failed ClickHouse/ES writes.

Fields:
- `messageId`: string/UUID.
- `event`: embedded `LikeEvent`.
- `target`: enum, `ClickHouse` or `Elasticsearch`.
- `attemptCount`: integer.
- `nextAttemptAtUtc`: timestamp.
- `lastError`: string, optional.

Rules:
- Redis toggle success creates or schedules downstream persistence/sync work.
- Failed downstream work must be retried until success or explicit dead-letter handling is added.
