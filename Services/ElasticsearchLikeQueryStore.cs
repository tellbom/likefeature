using likefeature.Models;
using Nest;

namespace likefeature.Services;

public class ElasticsearchLikeQueryStore : IElasticsearchLikeQueryStore
{
    private readonly IElasticClient _client;
    private readonly ILogger<ElasticsearchLikeQueryStore> _logger;

    private const string IndexName = "likes";

    public ElasticsearchLikeQueryStore(
        IElasticClient client,
        ILogger<ElasticsearchLikeQueryStore> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task UpsertAsync(string newsId, long likeCount)
    {
        var doc = new LikeQueryDocument
        {
            NewsId       = newsId,
            LikeCount    = likeCount,
            UpdatedAtUtc = DateTime.UtcNow
        };

        // A1 修复：UpdateAsync with DocAsUpsert(true) 为严格 upsert 语义
        // 文档存在时更新，不存在时插入，不会覆盖其他字段
        var response = await _client.UpdateAsync<LikeQueryDocument>(
            DocumentPath<LikeQueryDocument>.Id(newsId),
            u => u
                .Index(IndexName)
                .Doc(doc)
                .DocAsUpsert(true));

        if (!response.IsValid)
        {
            _logger.LogError(
                "ES upsert failed: newsId={NewsId} reason={Reason}",
                newsId, response.ServerError?.Error?.Reason ?? response.OriginalException?.Message);

            throw new InvalidOperationException(
                $"Elasticsearch upsert failed for newsId={newsId}: {response.ServerError?.Error?.Reason}");
        }

        _logger.LogInformation(
            "ES upsert success: newsId={NewsId} likeCount={LikeCount}",
            newsId, likeCount);
    }
}

/// <summary>ES 投影文档，对应 likes-index.json 的 mapping。</summary>
public class LikeQueryDocument
{
    public string NewsId { get; set; } = string.Empty;
    public long LikeCount { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
