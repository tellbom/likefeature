using Elasticsearch.Net;
using likefeature.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nest;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"]
            ?? throw new InvalidOperationException("Jwt:Authority is not configured.");
        options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("Jwt:RequireHttpsMetadata");
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

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
builder.Services.AddScoped<IRedisViewStore, RedisViewStore>();
builder.Services.AddScoped<IClickHouseViewEventWriter, ClickHouseViewEventWriter>();
builder.Services.AddScoped<IViewService, ViewService>();
builder.Services.AddScoped<IClickHouseRedisRecoveryReader, ClickHouseRedisRecoveryReader>();
builder.Services.AddScoped<IRedisRecoveryService, RedisRecoveryService>();

// 后台补偿 Worker（C2 修复：取消注释）
builder.Services.AddHostedService<LikeSyncRetryWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
