using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace SmartXWebApi.Filters;

/// <summary>STREAM-API-01: keep multipart bodies unread until the streaming action takes ownership (Microsoft, 2026d).</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class StreamingRequestAttribute : Attribute, IResourceFilter
{
    /// <summary>STREAM-API-02: remove form value providers that would otherwise consume the request before the action.</summary>
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        context.ValueProviderFactories.RemoveType<FormValueProviderFactory>();
        context.ValueProviderFactories.RemoveType<FormFileValueProviderFactory>();
        context.ValueProviderFactories.RemoveType<JQueryFormValueProviderFactory>();
    }
    /// <summary>STREAM-API-03: leave stream disposal to the request owner after action execution.</summary>
    public void OnResourceExecuted(ResourceExecutedContext context) { }
}
