using RealmCore.Resources.Overlay;
using RealmCore.Server.Extensions;

namespace RealmCore.Console.Commands;


[CommandName("display3ring")]
public sealed class Display3dRing : IIngameCommand
{
    private readonly ILogger<Display3dRing> _logger;
    private readonly IOverlayService _overlayService;

    public Display3dRing(ILogger<Display3dRing> logger, IOverlayService overlayService)
    {
        _logger = logger;
        _overlayService = overlayService;
    }

    public Task Handle(Entity entity, string[] args)
    {
        _overlayService.AddRing3dDisplay(entity, entity.Transform.Position, TimeSpan.FromSeconds(3));
        return Task.CompletedTask;
    }
}
