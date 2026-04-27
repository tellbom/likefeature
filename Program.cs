using Elasticsearch.Net;
using likefeature.Services;
using Nest;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(
        builder.Configuration["Redis:ConnectionString"]
            ?? throw new InvalidOperationException("Redis:ConnectionString is not configured.")));

// ClickHouse
builder.Services.AddSingleton(_ =>
    new ClickHouse.Client.ADO.ClickHouseConnection(
        builder.Configuration["ClickHouse:ConnectionString"]
            ?? throw new InvalidOperationException("ClickHouse:ConnectionString is not configured.")));

// Elasticsearch (NEST 7.x)
builder.Services.AddSingleton<IElasticClient>(_ =>
{
    var uri = builder.Configuration["Elasticsearch:Uri"]
              ?? throw new InvalidOperationException("Elasticsearch:Uri is not configured.");
    var settings = new ConnectionSettings(new Uri(uri))
        .DefaultIndex("likes");
    return new ElasticClient(settings);
});

// 重试队列：Singleton，跨 scope 共享 channel
builder.Services.AddSingleton<ILikeSyncRetryQueue, LikeSyncRetryQueue>();

// 业务服务：Scoped
builder.Services.AddScoped<IRedisLikeStateStore, RedisLikeStateStore>();
builder.Services.AddScoped<IClickHouseLikeEventWriter, ClickHouseLikeEventWriter>();
builder.Services.AddScoped<IElasticsearchLikeQueryStore, ElasticsearchLikeQueryStore>();
builder.Services.AddScoped<ILikeService, LikeService>();

// 后台补偿 Worker（C2 修复：取消注释）
builder.Services.AddHostedService<LikeSyncRetryWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
