using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace SmartXMVC.Filters;

/// <summary>STREAM-MVC-01: keep multipart bodies unread until the streaming action takes ownership (Microsoft, 2026d).</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class StreamingRequestAttribute : Attribute, IResourceFilter
{
    /// <summary>STREAM-MVC-02: remove form value providers that would otherwise consume the request before the action.</summary>
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        context.ValueProviderFactories.RemoveType<FormValueProviderFactory>();
        context.ValueProviderFactories.RemoveType<FormFileValueProviderFactory>();
        context.ValueProviderFactories.RemoveType<JQueryFormValueProviderFactory>();
    }
    /// <summary>STREAM-MVC-03: leave stream disposal to the request owner after action execution.</summary>
    public void OnResourceExecuted(ResourceExecutedContext context) { }
}
