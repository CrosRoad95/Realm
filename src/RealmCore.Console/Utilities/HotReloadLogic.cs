using RealmCore.Resources.AgnosticGuiSystem;
using System.Diagnostics;

namespace RealmCore.Console.Utilities;

public sealed class HotReloadLogic
{
    private static HotReloadLogic? _hotReloadLogic;
    private readonly HotReload _hotReload;
    private readonly IAgnosticGuiSystemService _agnosticGuiSystemService;
    private readonly ILogger<HotReloadLogic> _logger;

    public HotReloadLogic(IAgnosticGuiSystemService agnosticGuiSystemService, ILogger<HotReloadLogic> logger, string path)
    {
        _hotReloadLogic = this;
        _hotReload = new HotReload(path);
        _hotReload.OnReload += HandleHotReload;
        _agnosticGuiSystemService = agnosticGuiSystemService;
        _logger = logger;
    }

    private async void HandleHotReload()
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            await _agnosticGuiSystemService.UpdateGuiFiles();
            _logger.LogInformation("Updated guis in: {time}ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update gui files.");
        }
    }
}