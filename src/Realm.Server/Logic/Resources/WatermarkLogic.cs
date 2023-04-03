using Microsoft.Extensions.Options;
using Realm.Domain.Options;
using SlipeServer.Resources.Watermark;

namespace Realm.Server.Logic.Resources;

internal sealed class WatermarkLogic
{
    public WatermarkLogic(WatermarkService watermarkService, IOptions<GameplayOptions> options)
    {
        watermarkService.SetContent(options.Value.Watermark);
    }
}
