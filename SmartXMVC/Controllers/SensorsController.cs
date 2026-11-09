using Microsoft.AspNetCore.Mvc;
namespace SmartXMVC.Controllers;
/// <summary>PAGE-SENSORS: navigation routes for the Smart-X sensors workspace.</summary>
public sealed class SensorsController : Controller
{
    /// <summary>PAGE-SENSORS-01: render the index workspace; data is fetched asynchronously through the MVC gateway.</summary>
    public IActionResult Index() => View();
    /// <summary>PAGE-SENSORS-02: render the create workspace; data is fetched asynchronously through the MVC gateway.</summary>
    public IActionResult Create() => View();
    /// <summary>PAGE-SENSORS-03: render the edit workspace; data is fetched asynchronously through the MVC gateway.</summary>
    public IActionResult Edit() => View();
    /// <summary>PAGE-SENSORS-04: render the details workspace; data is fetched asynchronously through the MVC gateway.</summary>
    public IActionResult Details() => View();
}
