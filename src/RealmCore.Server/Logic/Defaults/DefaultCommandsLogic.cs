namespace RealmCore.Server.Logic.Defaults;

public class DefaultCommandsLogic
{
    public DefaultCommandsLogic(RealmCommandService commandService, IServiceProvider serviceProvider, IEnumerable<IInGameCommand> ingameCommands, ILogger<DefaultCommandsLogic> logger)
    {
        foreach (var inGameCommand in ingameCommands)
        {
            var type = inGameCommand.GetType();
            var commandNameAttribute = type.GetCustomAttribute<CommandNameAttribute>();
            if (commandNameAttribute == null)
            {
                logger.LogWarning($"Command class {type.Name} has no CommandName attribute");
                continue;
            }

            var commandName = commandNameAttribute.Name.ToLower();
            commandService.AddAsyncCommandHandler(commandName, async (element, args, token) =>
            {
                if (serviceProvider.GetRequiredService(type) is not IInGameCommand inGameCommand)
                    throw new InvalidOperationException();

                await inGameCommand.Handle(element, args);
            });
        }
    }
}
