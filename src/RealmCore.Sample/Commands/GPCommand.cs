﻿using RealmCore.ECS;

namespace RealmCore.Console.Commands;

[CommandName("gp")]
public sealed class GPCommand : IInGameCommand
{
    private readonly ILogger<GPCommand> _logger;

    public GPCommand(ILogger<GPCommand> logger)
    {
        _logger = logger;
    }

    public Task Handle(Entity entity, CommandArguments args)
    {
        var pos = entity.Transform.Position;
        var rot = entity.Transform.Rotation;
        _logger.LogInformation("{@x},{@y},{@z},{@rx},{@ry},{@rz}", pos.X, pos.Y, pos.Z, rot.X, rot.Y, rot.Z);
        return Task.CompletedTask;
    }
}