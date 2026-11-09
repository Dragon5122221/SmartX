using SmartX.Shared.Realtime;
namespace SmartXWebApi.Controllers;

[ApiController, Route("api/stream")]
public sealed class StreamController(GatewayService gateway, TelemetryDispatcher dispatcher) : ControllerBase
{
    [HttpGet] public IActionResult Snapshot(string? search, string? category, string? health, string? selected, int page = 1)
        => Ok(gateway.Stream(dispatcher.Status(),search,category,health,selected,page));
    [HttpPost("search")] public IActionResult Search(SearchRequest request) { gateway.RecordSearch(request); return Ok(new { recorded = true }); }
    [HttpPost("commands")] public IActionResult Command(CommandRequest request) => Ok(gateway.Command(request));
    [HttpPost("undo/{id:guid}")] public IActionResult Undo(Guid id) => Ok(gateway.Undo(id));
}
