using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
namespace SmartXWebApi.BackgroundServices;

/// <summary>Device emulator: live samples cross HTTP and the same queue as external devices.</summary>
public sealed class SimulationBackgroundService(GatewayService gateway, IHttpClientFactory clients,
    IServer server, IConfiguration configuration, ILogger<SimulationBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var batches = new List<object>(); gateway.Tick(batches.Add);
                string? endpoint = configuration["SmartX:SimulationApiUrl"];
                if (string.IsNullOrWhiteSpace(endpoint))
                {
                    endpoint = server.Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault(a=>a.StartsWith("http://",StringComparison.Ordinal));
                    endpoint = endpoint?.Replace("0.0.0.0","127.0.0.1").Replace("[::]","localhost");
                }
                if (endpoint is null) { logger.LogWarning("Set SmartX:SimulationApiUrl to a reachable API HTTP address."); continue; }
                using var http = clients.CreateClient("device-simulator");
                foreach (var batch in batches)
                {
                    try
                    {
                        using var response = batch switch
                        {
                            TelemetryBatchRequest<float> f => await http.PostAsJsonAsync(endpoint.TrimEnd('/')+"/api/telemetry/float",f,stoppingToken),
                            TelemetryBatchRequest<int> i => await http.PostAsJsonAsync(endpoint.TrimEnd('/')+"/api/telemetry/integer",i,stoppingToken),
                            TelemetryBatchRequest<bool> b => await http.PostAsJsonAsync(endpoint.TrimEnd('/')+"/api/telemetry/boolean",b,stoppingToken),
                            _ => throw new InvalidOperationException("Unknown telemetry type.")
                        };
                        if (!response.IsSuccessStatusCode) logger.LogWarning("Device ingress returned {Status}",response.StatusCode);
                    }
                    catch (HttpRequestException ex) { logger.LogWarning(ex,"Device emulator will retry on its next tick."); }
                    catch (TaskCanceledException) when (!stoppingToken.IsCancellationRequested) { logger.LogWarning("Device ingress timed out; retry next tick."); }
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }
}
