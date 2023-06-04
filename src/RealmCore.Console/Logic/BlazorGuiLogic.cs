namespace RealmCore.Console.Logic;

internal class BlazorGuiLogic
{
    private readonly ILogger<BlazorGuiLogic> _logger;

    public BlazorGuiLogic(IBlazorGuiService blazorGuiService, ILogger<BlazorGuiLogic> logger)
    {
        blazorGuiService.InvokeVoidAsync = HandleInvokeVoidAsync;
        blazorGuiService.InvokeAsync = HandleInvokeAsync;
        _logger = logger;
    }

    private void HandleInvokeVoidAsync(BlazorGuiComponent blazorGuiComponent, string kind, string identifier, string args)
    {
        _logger.LogInformation("HandleInvokeVoidAsync: kind={kind} {identifier}={args}", kind, identifier, args);
    }

    int a = 0;
    private async Task<object> HandleInvokeAsync(BlazorGuiComponent blazorGuiComponent, string kind, string identifier, string args)
    {
        _logger.LogInformation("HandleInvokeAsync: kind={kind} {identifier}={args}", kind, identifier, args);
        return new
        {
            a = ++a,
            b = 2
        };
    }
}
