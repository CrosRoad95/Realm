using Realm.Server.Services;
using Serilog;

namespace Realm.Console.Logic;

internal sealed class CommandsLogic
{
    private readonly RPGCommandService _commandService;
    private readonly ILogger _logger;

    public CommandsLogic(RPGCommandService commandService, ILogger logger)
    {
        _commandService = commandService;
        _logger = logger;
        _commandService.AddCommandHandler("gp", (entity, args) =>
        {
            logger.Information("{position}", entity.Transform.Position);
            return Task.CompletedTask;
        });
    }
}
