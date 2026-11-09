using Microsoft.AspNetCore.Mvc;
namespace SmartXMVC.Controllers;
/// <summary>PAGE-TELEMETRY: navigation routes for the Smart-X telemetry workspace.</summary>
public sealed class TelemetryController : Controller
{
    /// <summary>PAGE-TELEMETRY-01: render the index workspace; data is fetched asynchronously through the MVC gateway.</summary>
    public IActionResult Index() => View();
    /// <summary>PAGE-TELEMETRY-02: render the history workspace; data is fetched asynchronously through the MVC gateway.</summary>
    public IActionResult History() => View();
}
