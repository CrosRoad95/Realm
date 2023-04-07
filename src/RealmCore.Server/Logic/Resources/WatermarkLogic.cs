namespace RealmCore.Server.Logic.Resources;

internal sealed class WatermarkLogic
{
    public WatermarkLogic(WatermarkService watermarkService, IOptions<GameplayOptions> options)
    {
        watermarkService.SetContent(options.Value.Watermark);
    }
}
