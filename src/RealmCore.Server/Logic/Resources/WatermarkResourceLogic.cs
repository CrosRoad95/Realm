namespace RealmCore.Server.Logic.Resources;

internal sealed class WatermarkResourceLogic
{
    public WatermarkResourceLogic(WatermarkService watermarkService, IOptions<GameplayOptions> options)
    {
        watermarkService.SetContent(options.Value.Watermark);
    }
}
