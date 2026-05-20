# Quickstart: 点赞功能微服务

## Prerequisites

- .NET 6 SDK
- Redis reachable from the service
- ClickHouse reachable from the service
- Elasticsearch reachable from the service

## Configuration

Add service configuration to `appsettings.Development.json`:

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "ClickHouse": {
    "ConnectionString": "Host=localhost;Port=8123;Database=default"
  },
  "Elasticsearch": {
    "Uri": "http://localhost:9200"
  },
  "Jwt": {
    "Authority": "https://your-sso-host",
    "RequireHttpsMetadata": false
  }
}
```

## Run

```powershell
dotnet build
dotnet run
```

## Smoke Tests

This repository exposes backend APIs only. Frontend integration happens in separate projects.

Toggle a like:

```powershell
Invoke-RestMethod -Method Post `
  -Uri http://localhost:5000/api/likes/toggle `
  -Headers @{ "Authorization" = "Bearer <jwt-with-sub-user-001>" } `
  -ContentType "application/json" `
  -Body '{ "newsId": "news-001" }'
```

Check status:

```powershell
Invoke-RestMethod -Method Get `
  -Uri "http://localhost:5000/api/likes/status?newsId=news-001" `
  -Headers @{ "Authorization" = "Bearer <jwt-with-sub-user-001>" }
```

Check count:

```powershell
Invoke-RestMethod -Method Get `
  -Uri "http://localhost:5000/api/likes/count?newsId=news-001"
```

## Expected Behavior

- First toggle for a user/news pair returns `liked: true` and event type `Liked`.
- Second toggle for the same user/news pair returns `liked: false` and event type `Unliked`.
- Toggle success depends on Redis Lua execution, not synchronous ClickHouse/ES completion.
- ClickHouse/ES failures are retried by the compensation worker.
