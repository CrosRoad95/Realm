using Microsoft.Extensions.Logging;
using Realm.Resources.AgnosticGuiSystem;
using System.Diagnostics;

namespace Realm.ConsoleUtilities;

public sealed class HotReloadLogic
{
    private static HotReloadLogic? _hotReloadLogic;
    private readonly HotReload _hotReload;
    private readonly AgnosticGuiSystemService _agnosticGuiSystemService;
    private readonly ILogger _logger;

    public HotReloadLogic(AgnosticGuiSystemService agnosticGuiSystemService, ILogger<HotReloadLogic> logger, string path)
    {
        _hotReloadLogic = this;
        _hotReload = new HotReload(path);
        _hotReload.OnReload += HandleHotReload;
        _agnosticGuiSystemService = agnosticGuiSystemService;
        _logger = logger;
    }

    private async void HandleHotReload()
    {
        var stopwatch = Stopwatch.StartNew();
        await _agnosticGuiSystemService.UpdateGuiFiles();
        _logger.LogInformation("Updated guis in: {time}ms", stopwatch.ElapsedMilliseconds);
    }
}