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

    private Task HandleInvokeVoidAsync(BrowserGuiComponent blazorGuiComponent, string identifier, string args)
    {
        _logger.LogInformation("HandleInvokeVoidAsync: {identifier}={args}", identifier, args);
        return Task.CompletedTask;
    }

    int a = 0;
    private async Task<object?> HandleInvokeAsync(BrowserGuiComponent blazorGuiComponent, string identifier, string args)
    {
        _logger.LogInformation("HandleInvokeAsync: {identifier}={args}", identifier, args);
        return new
        {
            a = ++a,
            b = 2
        };
    }
}
