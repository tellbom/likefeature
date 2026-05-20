# Implementation Plan: 点赞功能微服务

**Branch**: `main` | **Date**: 2026-04-27 | **Spec**: `specs/main/spec.md`
**Input**: Feature specification from `specs/main/spec.md`

## Summary

实现一个 ASP.NET Core 点赞微服务，提供点赞/取消、点赞状态查询、新闻点赞数查询三个后端 HTTP API，供其他前端项目调用；本仓库不实现前端页面或 UI。Redis 作为当前点赞状态权威源，使用 Lua 脚本原子完成 toggle 和计数更新；ClickHouse 只追加 `Liked` / `Unliked` 历史事件；Elasticsearch 作为异步生成的查询视图。Redis toggle 成功即返回成功，ClickHouse/ES 失败进入可靠重试/补偿流程。

## Technical Context

**Language/Version**: C# / .NET 6.0  
**Primary Dependencies**: ASP.NET Core Web API, StackExchange.Redis, ClickHouse client, Elasticsearch .NET client, background hosted services  
**Storage**: Redis current-state cache, ClickHouse insert-only event history, Elasticsearch aggregate/query index  
**Testing**: .NET test project with unit tests for services and contract/integration tests for API behavior  
**Target Platform**: Server-side ASP.NET Core Web API consumed by external frontend projects  
**Project Type**: Web service / microservice  
**Performance Goals**: Toggle API should complete from Redis path without waiting for ClickHouse/ES persistence; Redis Lua path should be suitable for high-concurrency requests  
**Constraints**: Redis is the authoritative current state; ClickHouse writes are insert-only; ES is derived from async sync; user ID comes from the authenticated JWT subject claim  
**Scale/Scope**: Single backend like microservice with three public endpoints and internal retry/sync workers; no frontend implementation in this repository

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The constitution file still contains template placeholders and no enforceable project-specific gates. Treat this as pass with a process risk: before broader team delivery, replace placeholder constitution principles with concrete testing, observability, API, and operational standards.

Post-design re-check: pass under the same caveat. The design includes explicit API contracts, data model, retry behavior, and test targets, so no known feature-level violation is present.

## Project Structure

### Documentation (this feature)

```text
specs/main/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── likes-api.openapi.yaml
└── spec.md
```

### Source Code (repository root)

```text
Controllers/
└── LikesController.cs

Models/
├── LikeEvent.cs
├── LikeEventType.cs
├── LikeStatus.cs
└── ApiResponse.cs

Services/
├── ILikeService.cs
├── LikeService.cs
├── RedisLikeStateStore.cs
├── ClickHouseLikeEventWriter.cs
├── ElasticsearchLikeQueryStore.cs
└── LikeSyncRetryWorker.cs

Infrastructure/
├── Redis/
│   └── like-toggle.lua
├── ClickHouse/
│   └── likes_events.sql
└── Elasticsearch/
    └── likes-index.json

Tests/
├── Unit/
├── Contract/
└── Integration/
```

**Structure Decision**: Keep the existing ASP.NET Core project as a single Web API service. Add controllers, service abstractions, storage adapters, infrastructure scripts, and tests at the repository root rather than introducing a multi-project architecture for the first implementation.

## Phase 0: Research

Research outcomes are captured in `specs/main/research.md`. No unresolved `NEEDS CLARIFICATION` items remain for planning.

## Phase 1: Design & Contracts

Design artifacts generated:

- `specs/main/data-model.md`
- `specs/main/contracts/likes-api.openapi.yaml`
- `specs/main/quickstart.md`

## Complexity Tracking

No constitution violations or extra complexity exceptions are required at this stage.
