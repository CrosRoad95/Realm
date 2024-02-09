namespace RealmCore.Server.Modules.Players.Gui.Dx;

internal sealed class WatermarkResourceLogic
{
    public WatermarkResourceLogic(WatermarkService watermarkService, IOptions<GameplayOptions> options)
    {
        watermarkService.SetContent(options.Value.Watermark);
    }
}
