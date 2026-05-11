using LendingSystem.Application.Abstractions;
using LendingSystem.Application.System;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class SystemController(SystemStatusService system, IVideoDownloadClient videos) : ControllerBase
{
    [HttpGet("/api/health")]
    public ActionResult<ServiceHealthResponse> Health() => Ok(system.GetHealth());

    [HttpGet("/api/status")]
    public async Task<ActionResult<SystemStatusResponse>> Status(CancellationToken cancellationToken)
    {
        var response = await system.GetStatusAsync(cancellationToken);
        return response.Status == "ok" ? Ok(response) : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }

    [HttpGet("/api/download")]
    public async Task<IActionResult> Download([FromQuery] string? url, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var sourceUrl))
        {
            return BadRequest(new { error = "url query parameter must be a valid absolute URL" });
        }

        Response.ContentType = "application/octet-stream";
        Response.Headers.ContentDisposition = "attachment; filename=\"downloaded_video.mp4\"";

        var wroteChunk = false;
        await foreach (var chunk in videos.DownloadAndStreamAsync(sourceUrl, cancellationToken))
        {
            if (!string.IsNullOrWhiteSpace(chunk.FileName) && !wroteChunk)
            {
                Response.Headers.ContentDisposition = $"attachment; filename=\"{SanitizeHeaderFilename(chunk.FileName)}\"";
            }

            if (!string.IsNullOrWhiteSpace(chunk.ErrorMessage))
            {
                if (!wroteChunk)
                {
                    return StatusCode(StatusCodes.Status502BadGateway, new { error = chunk.ErrorMessage });
                }

                break;
            }

            if (chunk.Bytes is null || chunk.Bytes.Length == 0)
            {
                continue;
            }

            await Response.Body.WriteAsync(chunk.Bytes, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
            wroteChunk = true;
        }

        return new EmptyResult();
    }

    private static string SanitizeHeaderFilename(string filename)
    {
        var sanitized = filename
            .Replace("\\", "_", StringComparison.Ordinal)
            .Replace("\"", "_", StringComparison.Ordinal)
            .Replace("\r", "_", StringComparison.Ordinal)
            .Replace("\n", "_", StringComparison.Ordinal)
            .Trim();

        return string.IsNullOrWhiteSpace(sanitized) ? "downloaded_video.mp4" : sanitized;
    }
}
