namespace SmartXWebApi.Services;
/// <summary>ERR-01: expected gateway failure carrying an intentional HTTP status.</summary>
public sealed class GatewayException(string message, int status = 400) : Exception(message)
{
    public int Status { get; } = status;
}
