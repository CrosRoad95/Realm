namespace RealmCore.Console.Logic;

internal class BlazorGuiLogic
{
    private readonly ILogger<BlazorGuiLogic> _logger;

    public BlazorGuiLogic(IBlazorGuiService blazorGuiService, ILogger<BlazorGuiLogic> logger)
    {
        blazorGuiService.InvokeVoidAsync += HandleInvokeVoidAsync;
        _logger = logger;
    }

    private void HandleInvokeVoidAsync(BlazorGuiComponent blazorGuiComponent, string identifier, string args)
    {
        _logger.LogInformation("InvokeVoidAsync: {identifier}={args}", identifier, args);
    }
}
