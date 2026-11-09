using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection;
using SmartXWebApi.BackgroundServices;
using SmartXWebApi.Storage;

// BOOT-API-01: framework wiring; application state lives in one synchronized gateway singleton.
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(o => { o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); o.JsonSerializerOptions.MaxDepth = 100; });
builder.Services.AddOpenApi();
builder.Services.AddDataProtection().SetApplicationName("SmartX.Part1");
builder.Services.AddSingleton<GatewayService>();
builder.Services.AddSingleton<TelemetryDispatcher>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<TelemetryDispatcher>());
builder.Services.AddHttpClient("device-simulator");
builder.Services.AddSingleton<AttachmentKeyProvider>();
builder.Services.AddSingleton<EncryptedAttachmentStorage>();
builder.Services.AddHostedService<SimulationBackgroundService>();
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 9 * 1024 * 1024);
var app = builder.Build();
// HTTP-ERROR-01: consistently translate expected failures, without exposing stack traces or keys.
app.Use(async (context, next) =>
{
    try { await next(context); }
    catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
    catch (Exception ex)
    {
        if (context.Response.HasStarted) throw;
        int status = ex is GatewayException known ? known.Status : ex is BadHttpRequestException bad ? bad.StatusCode : ex is InvalidDataException ? 400 : 500;
        if (status == 500) app.Logger.LogError(ex, "Gateway operation failed.");
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = status == 500 ? "Gateway operation failed" : "Request could not be completed",
            Detail = status == 500 ? "The operation failed safely. Check API logs and protected storage configuration." : ex.Message
        });
    }
});
if (app.Environment.IsDevelopment()) app.MapOpenApi();
if (!app.Configuration.GetValue<bool>("SmartX:AllowHttp")) app.UseHttpsRedirection();
app.MapControllers();
app.Run();
