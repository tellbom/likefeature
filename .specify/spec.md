# 点赞功能微服务

## Clarifications
### Session 2026-04-27
- Q: 并发点赞/取消时，系统应以哪个存储作为“当前点赞状态”的权威来源？ -> A: Redis 是当前状态权威；ClickHouse 只记录事件历史；ES 由异步同步生成查询视图
- Q: Redis 中的点赞/取消切换应如何保证原子性？ -> A: 使用 Redis Lua 脚本原子完成状态判断、写入和计数更新
- Q: ClickHouse 的 insert-only 历史表应记录哪种事件模型？ -> A: 每次 toggle 插入一条事件，事件类型为 Liked 或 Unliked
- Q: Redis toggle 成功后，如果 ClickHouse 或 ES 写入/同步暂时失败，接口应如何处理？ -> A: 返回成功；失败的 ClickHouse/ES 写入进入可靠重试/补偿队列
- Q: 点赞接口中的用户 ID 应由哪里提供？ -> A: 开发阶段使用请求头中的用户 ID，后续接入 JWT 校验

## 功能描述
1. 用户对每篇新闻点赞，每个工号只能点赞一次。
2. 再次点击已点赞的新闻，执行取消点赞。
3. 高并发请求下，前端需要快速响应。
4. 点赞状态缓存 Redis，历史存储 ClickHouse，ES 提供聚合查询。

## 约束
- 点赞/取消必须更新 Redis 热点缓存。
- Redis 是当前点赞状态的权威来源；ClickHouse 只记录点赞/取消事件历史；ES 由异步同步生成查询视图。
- 点赞/取消切换必须通过 Redis Lua 脚本原子完成状态判断、用户点赞集合写入和点赞计数更新。
- ClickHouse 仅允许 insert-only 写操作；每次 toggle 必须追加一条事件，事件类型为 `Liked` 或 `Unliked`。
- Redis toggle 成功后接口即可返回成功；ClickHouse/ES 写入或同步失败时，必须进入可靠重试/补偿队列。
- 开发阶段用户 ID 从请求头读取；后续接入 JWT 后，用户 ID 改为从认证上下文/Token Claim 获取。
- 微服务 API 命名与返回格式必须遵循规范：
  - POST /api/likes/toggle
  - GET /api/likes/status
  - GET /api/likes/count
- 点赞操作必须记录用户 ID 与新闻 ID，避免重复操作。
