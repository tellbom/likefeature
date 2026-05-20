using likefeature.Models;
using likefeature.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace likefeature.Controllers;

[ApiController]
[Authorize]
[Route("api/views")]
public class ViewsController : ControllerBase
{
    private readonly IViewService _viewService;
    private readonly ILogger<ViewsController> _logger;

    public ViewsController(IViewService viewService, ILogger<ViewsController> logger)
    {
        _viewService = viewService;
        _logger = logger;
    }

    [HttpPost("record")]
    public async Task<IActionResult> Record([FromBody] RecordViewRequest request)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new { message = "JWT preferred_username claim is required." });

        if (string.IsNullOrWhiteSpace(request?.NewsId))
            return BadRequest(new { message = "newsId is required." });

        _logger.LogInformation("View record called: userId={UserId} newsId={NewsId}", userId, request.NewsId);

        var (recorded, viewCount) = await _viewService.RecordAsync(userId, request.NewsId);

        return Ok(new RecordViewResponse
        {
            Success = true,
            NewsId = request.NewsId,
            Recorded = recorded,
            ViewCount = viewCount
        });
    }

    [HttpGet("count")]
    public async Task<IActionResult> Count([FromQuery] string newsId)
    {
        if (string.IsNullOrWhiteSpace(newsId))
            return BadRequest(new { message = "newsId is required." });

        var count = await _viewService.GetCountAsync(newsId);

        return Ok(new ViewCountResponse
        {
            Success = true,
            NewsId = newsId,
            ViewCount = count
        });
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue("preferred_username");
    }
}

public class RecordViewRequest
{
    public string NewsId { get; set; } = string.Empty;
}
