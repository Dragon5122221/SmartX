using Microsoft.AspNetCore.Mvc;
namespace SmartXMVC.Controllers;
/// <summary>PAGE-DEPLOYMENT: navigation routes for the Smart-X deployment workspace.</summary>
public sealed class DeploymentController : Controller
{
    /// <summary>PAGE-DEPLOYMENT-01: render the index workspace; data is fetched asynchronously through the MVC gateway.</summary>
    public IActionResult Index() => View();
}
