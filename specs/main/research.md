# Research: 点赞功能微服务

## Decision: Redis is the current-state authority

Rationale: The feature requires fast frontend response under high concurrency. Redis can maintain per-news liked user sets and count values with low-latency atomic operations. ClickHouse and Elasticsearch remain downstream systems instead of competing state authorities.

Alternatives considered:
- ClickHouse as authority: rejected because insert-only history is not suitable for synchronous current-state checks without replay/aggregation.
- Elasticsearch as authority: rejected because ES is better used as a query view and can lag during indexing.
- Separate relational authority: rejected for this scope because it adds another storage dependency not required by the spec.

## Decision: Redis Lua script for toggle atomicity

Rationale: Lua keeps membership check, set mutation, count update, and result return in one Redis atomic execution. This avoids race conditions around duplicate likes and count drift.

Alternatives considered:
- WATCH/MULTI/EXEC: valid but more retry-heavy under hot keys.
- Distributed lock: adds latency and operational complexity.
- Async correction only: risks visible incorrect state/count after concurrent toggles.

## Decision: ClickHouse stores insert-only `Liked` / `Unliked` events

Rationale: The feature explicitly constrains ClickHouse to insert-only writes. Event rows preserve audit history and allow downstream aggregation without mutating historical records.

Alternatives considered:
- Only liked events: rejected because cancellations would be invisible.
- Snapshot rows only: rejected because it loses per-user action history.
- Final-state rows without updates: rejected because multiple rows would still require conflict interpretation without preserving clear event semantics.

## Decision: ClickHouse/Elasticsearch failures use retry/compensation

Rationale: Redis toggle success defines API success. Durable retry work preserves frontend speed while keeping historical and query projections recoverable.

Alternatives considered:
- Require Redis, ClickHouse, and ES all to succeed: rejected because downstream storage outages would block the main user action.
- Ignore downstream failures: rejected because it loses audit/query consistency.

## Decision: User ID comes from request header during development

Rationale: The current feature is scoped before JWT integration. A required header lets services and tests implement user-specific behavior now while keeping the future JWT migration explicit.

Alternatives considered:
- Request body user ID: rejected because it leaks identity into command payloads.
- Token claim now: deferred until JWT integration is available.
