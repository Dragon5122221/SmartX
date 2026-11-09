using Microsoft.AspNetCore.Mvc;
namespace SmartXMVC.Controllers;
/// <summary>PAGE-ISSUES: navigation routes for the Smart-X issues workspace.</summary>
public sealed class IssuesController : Controller
{
    /// <summary>PAGE-ISSUES-01: render the index workspace; data is fetched asynchronously through the MVC gateway.</summary>
    public IActionResult Index() => View();
}
