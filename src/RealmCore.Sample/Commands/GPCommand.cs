namespace RealmCore.Sample.Commands;

[CommandName("gp")]
public sealed class GPCommand : IInGameCommand
{
    private readonly ILogger<GPCommand> _logger;

    public string[] RequiredPolicies { get; } = [];
    public GPCommand(ILogger<GPCommand> logger)
    {
        _logger = logger;
    }

    public Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        var pos = player.Position;
        var rot = player.Rotation;
        _logger.LogInformation("{@x},{@y},{@z},{@rx},{@ry},{@rz}", pos.X, pos.Y, pos.Z, rot.X, rot.Y, rot.Z);
        return Task.CompletedTask;
    }
}
