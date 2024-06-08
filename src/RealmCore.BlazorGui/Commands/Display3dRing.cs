namespace RealmCore.BlazorGui.Commands;

[CommandName("display3ring")]
public sealed class Display3dRing : IInGameCommand
{
    private readonly ILogger<Display3dRing> _logger;
    private readonly IOverlayService _overlayService;

    public string[] RequiredPolicies { get; } = [];
    public Display3dRing(ILogger<Display3dRing> logger, IOverlayService overlayService)
    {
        _logger = logger;
        _overlayService = overlayService;
    }

    public Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        _overlayService.AddRing3dDisplay(player, player.Position, TimeSpan.FromSeconds(3));
        return Task.CompletedTask;
    }
}
