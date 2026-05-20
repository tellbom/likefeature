using likefeature.Models;
using likefeature.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace likefeature.Controllers;

[ApiController]
[Route("api/likes")]
public class LikesController : ControllerBase
{
    private readonly ILikeService _likeService;
    private readonly ILogger<LikesController> _logger;

    public LikesController(ILikeService likeService, ILogger<LikesController> logger)
    {
        _likeService = likeService;
        _logger      = logger;
    }

    /// <summary>
    /// POST /api/likes/toggle
    /// Authorization: Bearer JWT (required)
    /// Body:   { "newsId": "..." }
    /// </summary>
    [Authorize]
    [HttpPost("toggle")]
    public async Task<IActionResult> Toggle([FromBody] ToggleRequest request)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new { message = "JWT preferred_username claim is required." });

        if (string.IsNullOrWhiteSpace(request?.NewsId))
            return BadRequest(new { message = "newsId is required." });

        _logger.LogInformation("Toggle called: userId={UserId} newsId={NewsId}", userId, request.NewsId);

        var result = await _likeService.ToggleAsync(userId, request.NewsId);

        return Ok(new ToggleLikeResponse
        {
            Success   = true,
            NewsId    = result.NewsId,
            Liked     = result.Liked,
            LikeCount = result.LikeCount,
            EventType = result.EventType.ToString()
        });
    }

    /// <summary>
    /// GET /api/likes/status?newsId=...
    /// Authorization: Bearer JWT (required)
    /// </summary>
    [Authorize]
    [HttpGet("status")]
    public async Task<IActionResult> Status([FromQuery] string newsId)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new { message = "JWT preferred_username claim is required." });

        if (string.IsNullOrWhiteSpace(newsId))
            return BadRequest(new { message = "newsId is required." });

        var liked = await _likeService.GetStatusAsync(userId, newsId);

        return Ok(new LikeStatusResponse
        {
            Success = true,
            NewsId  = newsId,
            Liked   = liked
        });
    }

    /// <summary>
    /// GET /api/likes/count?newsId=...
    /// </summary>
    [HttpGet("count")]
    public async Task<IActionResult> Count([FromQuery] string newsId)
    {
        if (string.IsNullOrWhiteSpace(newsId))
            return BadRequest(new { message = "newsId is required." });

        var count = await _likeService.GetCountAsync(newsId);

        return Ok(new LikeCountResponse
        {
            Success   = true,
            NewsId    = newsId,
            LikeCount = count
        });
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue("preferred_username");
    }
}

/// <summary>Request body for POST /api/likes/toggle</summary>
public class ToggleRequest
{
    public string NewsId { get; set; } = string.Empty;
}
