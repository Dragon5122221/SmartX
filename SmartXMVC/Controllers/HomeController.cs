using Microsoft.AspNetCore.Mvc;
namespace SmartXMVC.Controllers;
/// <summary>PAGE-HOME: navigation routes for the Smart-X home workspace.</summary>
public sealed class HomeController : Controller
{
    /// <summary>PAGE-HOME-01: render the index workspace; data is fetched asynchronously through the MVC gateway.</summary>
    public IActionResult Index() => View();
}
