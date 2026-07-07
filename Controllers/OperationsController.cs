using likefeature.Models;
using likefeature.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace likefeature.Controllers;

[ApiController]
[Authorize]
[Route("api/operations")]
public class OperationsController : ControllerBase
{
    private readonly IRedisRecoveryService _redisRecoveryService;
    private readonly ILogger<OperationsController> _logger;

    public OperationsController(
        IRedisRecoveryService redisRecoveryService,
        ILogger<OperationsController> logger)
    {
        _redisRecoveryService = redisRecoveryService;
        _logger = logger;
    }

    [HttpPost("recover/redis")]
    public async Task<IActionResult> RecoverRedis([FromBody] RedisRecoveryRequest? request)
    {
        request ??= new RedisRecoveryRequest();

        if (!request.RecoverLikes && !request.RecoverViews)
            return BadRequest(new { message = "At least one of recoverLikes or recoverViews must be true." });

        _logger.LogWarning(
            "Redis recovery endpoint called: recoverLikes={RecoverLikes} recoverViews={RecoverViews}",
            request.RecoverLikes,
            request.RecoverViews);

        var response = await _redisRecoveryService.RecoverAsync(request);
        return Ok(response);
    }
}
