# Tasks: 点赞功能微服务

**Input**: Design documents from `specs/main/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/likes-api.openapi.yaml`, `quickstart.md`
**Scope**: Backend API service only; no frontend, Razor page, or UI work is in scope for this feature.
**Tests**: Included because the implementation plan requires unit, contract, and integration tests.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare the existing ASP.NET Core project for backend API implementation.

- [ ] T001 Add backend dependency packages to `likefeature.csproj` for Redis, ClickHouse, Elasticsearch, and API documentation support
- [ ] T002 Create solution test structure in `Tests/Unit/`, `Tests/Contract/`, and `Tests/Integration/`
- [ ] T003 [P] Add service configuration sections for Redis, ClickHouse, Elasticsearch, and retry behavior in `appsettings.json`
- [ ] T004 [P] Add local development configuration examples for Redis, ClickHouse, and Elasticsearch in `appsettings.Development.json`
- [ ] T005 Update ASP.NET Core routing from Razor Pages template toward API controllers in `Program.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared models, interfaces, storage scripts, and infrastructure required by every user story.

**CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T006 [P] Create common API response models in `Models/ApiResponse.cs`
- [ ] T007 [P] Create like event enum in `Models/LikeEventType.cs`
- [ ] T008 [P] Create like event model in `Models/LikeEvent.cs`
- [ ] T009 [P] Create like status model in `Models/LikeStatus.cs`
- [ ] T010 Define service contract for toggle, status, and count operations in `Services/ILikeService.cs`
- [ ] T011 Define Redis state store contract and DTOs in `Services/IRedisLikeStateStore.cs`
- [ ] T012 Define ClickHouse event writer contract in `Services/IClickHouseLikeEventWriter.cs`
- [ ] T013 Define Elasticsearch query store contract in `Services/IElasticsearchLikeQueryStore.cs`
- [ ] T014 Define retry queue contract for downstream persistence failures in `Services/ILikeSyncRetryQueue.cs`
- [ ] T015 Create Redis Lua toggle script in `Infrastructure/Redis/like-toggle.lua`
- [ ] T016 [P] Create ClickHouse insert-only table schema in `Infrastructure/ClickHouse/likes_events.sql`
- [ ] T017 [P] Create Elasticsearch likes index mapping in `Infrastructure/Elasticsearch/likes-index.json`
- [ ] T018 Register service options, storage clients, controllers, and hosted workers in `Program.cs`

**Checkpoint**: Foundation ready; user story implementation can now begin.

---

## Phase 3: User Story 1 - Toggle Like State (Priority: P1) MVP

**Goal**: External frontend projects can call `POST /api/likes/toggle` to like or cancel like for a news item using a JWT Bearer token.

**Independent Test**: Send two toggle requests with the same JWT subject and `newsId`; first returns `liked: true` and `Liked`, second returns `liked: false` and `Unliked`, with non-negative count.

### Tests for User Story 1

- [ ] T019 [P] [US1] Add contract tests for `POST /api/likes/toggle` in `Tests/Contract/LikesToggleContractTests.cs`
- [ ] T020 [P] [US1] Add Redis Lua unit tests for duplicate-like and unlike transitions in `Tests/Unit/RedisLikeStateStoreTests.cs`
- [ ] T021 [P] [US1] Add integration test for two consecutive toggle calls in `Tests/Integration/LikesToggleIntegrationTests.cs`

### Implementation for User Story 1

- [ ] T022 [US1] Implement Redis Lua execution and result mapping in `Services/RedisLikeStateStore.cs`
- [ ] T023 [US1] Implement ClickHouse append-only event writer in `Services/ClickHouseLikeEventWriter.cs`
- [ ] T024 [US1] Implement retry queue enqueue behavior for ClickHouse/ES failures in `Services/LikeSyncRetryQueue.cs`
- [ ] T025 [US1] Implement toggle orchestration in `Services/LikeService.cs`
- [ ] T026 [US1] Implement `POST /api/likes/toggle` in `Controllers/LikesController.cs`
- [ ] T027 [US1] Add JWT authorization and request body validation for `newsId` in `Controllers/LikesController.cs`

**Checkpoint**: MVP toggle flow works independently and does not require frontend code.

---

## Phase 4: User Story 2 - Query User Like Status (Priority: P2)

**Goal**: External frontend projects can call `GET /api/likes/status` to know whether the authenticated user liked a news item.

**Independent Test**: Toggle a news item once, then call status with the same JWT subject and `newsId`; response returns `liked: true`.

### Tests for User Story 2

- [ ] T028 [P] [US2] Add contract tests for `GET /api/likes/status` in `Tests/Contract/LikesStatusContractTests.cs`
- [ ] T029 [P] [US2] Add unit tests for Redis status lookup in `Tests/Unit/RedisLikeStatusTests.cs`
- [ ] T030 [P] [US2] Add integration test for toggle then status query in `Tests/Integration/LikesStatusIntegrationTests.cs`

### Implementation for User Story 2

- [ ] T031 [US2] Implement Redis membership status lookup in `Services/RedisLikeStateStore.cs`
- [ ] T032 [US2] Implement status orchestration in `Services/LikeService.cs`
- [ ] T033 [US2] Implement `GET /api/likes/status` in `Controllers/LikesController.cs`
- [ ] T034 [US2] Add query validation for `newsId` and JWT authorization in `Controllers/LikesController.cs`

**Checkpoint**: Status query works independently for API consumers.

---

## Phase 5: User Story 3 - Query News Like Count (Priority: P3)

**Goal**: External frontend projects can call `GET /api/likes/count` to get the current like count for a news item.

**Independent Test**: Toggle likes for one news item and call count; response returns the current Redis count and never returns a negative number.

### Tests for User Story 3

- [ ] T035 [P] [US3] Add contract tests for `GET /api/likes/count` in `Tests/Contract/LikesCountContractTests.cs`
- [ ] T036 [P] [US3] Add unit tests for Redis count lookup in `Tests/Unit/RedisLikeCountTests.cs`
- [ ] T037 [P] [US3] Add integration test for toggle then count query in `Tests/Integration/LikesCountIntegrationTests.cs`

### Implementation for User Story 3

- [ ] T038 [US3] Implement Redis count lookup in `Services/RedisLikeStateStore.cs`
- [ ] T039 [US3] Implement count orchestration in `Services/LikeService.cs`
- [ ] T040 [US3] Implement `GET /api/likes/count` in `Controllers/LikesController.cs`
- [ ] T041 [US3] Add query validation for `newsId` in `Controllers/LikesController.cs`

**Checkpoint**: Count query works independently for API consumers.

---

## Phase 6: Downstream Sync & Compensation

**Purpose**: Make ClickHouse/ES persistence recoverable without blocking the API success path.

- [ ] T042 [P] Add unit tests for retry queue scheduling and retry limits in `Tests/Unit/LikeSyncRetryQueueTests.cs`
- [ ] T043 [P] Add unit tests for Elasticsearch projection writes in `Tests/Unit/ElasticsearchLikeQueryStoreTests.cs`
- [ ] T044 Implement Elasticsearch projection writer in `Services/ElasticsearchLikeQueryStore.cs`
- [ ] T045 Implement background retry and compensation worker in `Services/LikeSyncRetryWorker.cs`
- [ ] T046 Add integration test for downstream failure enqueue behavior in `Tests/Integration/LikeSyncRetryIntegrationTests.cs`

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, observability, validation, and final verification across all backend API stories.

- [ ] T047 [P] Update OpenAPI contract examples in `specs/main/contracts/likes-api.openapi.yaml`
- [ ] T048 [P] Update backend-only smoke test notes in `specs/main/quickstart.md`
- [ ] T049 Add structured logging around Redis toggle, ClickHouse event append, ES sync, and retry worker paths in `Services/LikeService.cs`
- [ ] T050 Add API error response consistency checks in `Tests/Contract/LikesErrorContractTests.cs`
- [ ] T051 Run `dotnet build` and record any required follow-up fixes in `specs/main/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- Setup (Phase 1): No dependencies.
- Foundational (Phase 2): Depends on Setup and blocks all user stories.
- US1 Toggle (Phase 3): Depends on Foundational; recommended MVP.
- US2 Status (Phase 4): Depends on Foundational and can use US1 for integration validation.
- US3 Count (Phase 5): Depends on Foundational and can use US1 for integration validation.
- Downstream Sync (Phase 6): Depends on US1 event creation path.
- Polish (Phase 7): Depends on selected stories being complete.

### User Story Dependencies

- US1: No dependency on other stories after Foundational.
- US2: Can be implemented after Foundational, but full journey validation benefits from US1.
- US3: Can be implemented after Foundational, but full journey validation benefits from US1.

### Parallel Opportunities

- T003 and T004 can run in parallel after project package decisions.
- T006 through T017 are mostly parallel by file once model naming is agreed.
- US1 tests T019 through T021 can be written in parallel.
- US2 tests T028 through T030 can be written in parallel.
- US3 tests T035 through T037 can be written in parallel.
- Downstream sync tests T042 and T043 can be written in parallel.

## Parallel Example: User Story 1

```text
Task: "Add contract tests for POST /api/likes/toggle in Tests/Contract/LikesToggleContractTests.cs"
Task: "Add Redis Lua unit tests for duplicate-like and unlike transitions in Tests/Unit/RedisLikeStateStoreTests.cs"
Task: "Add integration test for two consecutive toggle calls in Tests/Integration/LikesToggleIntegrationTests.cs"
```

## Implementation Strategy

### MVP First

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3 for `POST /api/likes/toggle`.
3. Validate with contract, unit, and integration tests.
4. Stop and verify the endpoint can be consumed by external frontend projects without adding frontend code here.

### Incremental Delivery

1. Deliver US1 toggle API.
2. Deliver US2 status API.
3. Deliver US3 count API.
4. Add downstream sync and compensation hardening.
5. Finish observability, contract examples, and quickstart validation.
