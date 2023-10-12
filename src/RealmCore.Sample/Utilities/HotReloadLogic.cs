using System.Diagnostics;

namespace RealmCore.Sample.Utilities;

public sealed class HotReloadLogic
{
    private readonly HotReload _hotReload;
    private readonly IGuiSystemService _GuiSystemService;
    private readonly ILogger<HotReloadLogic> _logger;

    public HotReloadLogic(IGuiSystemService GuiSystemService, ILogger<HotReloadLogic> logger, string path)
    {
        _hotReload = new HotReload(path);
        _hotReload.OnReload += HandleHotReload;
        _GuiSystemService = GuiSystemService;
        _logger = logger;
    }

    private async void HandleHotReload()
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            await _GuiSystemService.UpdateGuiFiles();
            _logger.LogInformation("Updated guis in: {time}ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update gui files.");
        }
    }
}