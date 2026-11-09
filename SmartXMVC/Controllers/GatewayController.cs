using Microsoft.AspNetCore.Mvc;
using SmartXMVC.Services;
using System.Text.RegularExpressions;
namespace SmartXMVC.Controllers;

/// <summary>CLIENT-HTTP: same-origin MVC gateway; only fixed backend routes are forwarded.</summary>
[Route("gateway"), AutoValidateAntiforgeryToken]
public sealed partial class GatewayController(SmartXApiClient client) : Controller
{
    /// <summary>CLIENT-HTTP-01: stream JSON/multipart requests and results with cancellation and safe outage feedback.</summary>
    [SmartXMVC.Filters.StreamingRequest]
    [AcceptVerbs("GET", "POST", "PUT"), Route("{**path}"), RequestSizeLimit(9 * 1024 * 1024)]
    public async Task Forward(string? path, CancellationToken token)
    {
        if (path is null || !SafePath().IsMatch(path)) { Response.StatusCode = 404; return; }
        try
        {
            using var result = await client.SendAsync(path, Request, token);
            Response.StatusCode = (int)result.StatusCode;
            Response.ContentType = result.Content.Headers.ContentType?.ToString() ?? "application/json";
            if (result.Content.Headers.ContentDisposition is { } disposition) Response.Headers.ContentDisposition = disposition.ToString();
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            await result.Content.CopyToAsync(Response.Body, token);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            if (token.IsCancellationRequested) return;
            if (Response.HasStarted) throw;
            Response.StatusCode = 503;
            await Response.WriteAsJsonAsync(new { title = "Gateway unavailable", detail = "Start SmartXWebApi and check its HTTPS address and development certificate, then retry." }, token);
        }
    }
    /// <summary>CLIENT-HTTP-02: reject absolute URLs, traversal and arbitrary upstream resource names.</summary>
    [GeneratedRegex(@"^(health|sensors|telemetry|deployment|dashboard|simulation|stream)(/[A-Za-z0-9_:\-]+)*$")]
    private static partial Regex SafePath();
}
