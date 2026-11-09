using Microsoft.AspNetCore.Mvc;
namespace SmartXMVC.Controllers;
/// <summary>PAGE-DASHBOARD: navigation routes for the Smart-X dashboard workspace.</summary>
public sealed class DashboardController : Controller
{
    /// <summary>PAGE-DASHBOARD-01: render the index workspace; data is fetched asynchronously through the MVC gateway.</summary>
    public IActionResult Index() => View();
}
