namespace RealmCore.Server.Logic.Defaults;

public class DefaultCommandsLogic
{
    public DefaultCommandsLogic(RealmCommandService commandService, IServiceProvider serviceProvider, IEnumerable<IInGameCommand> ingameCommands, ILogger<DefaultCommandsLogic> logger)
    {
        foreach (var ingameCommand in ingameCommands)
        {
            var type = ingameCommand.GetType();
            var commandNameAttribute = type.GetCustomAttribute<CommandNameAttribute>();
            if (commandNameAttribute == null)
            {
                logger.LogWarning($"Command class {type.Name} has no CommandName attribute");
                continue;
            }

            var commandName = commandNameAttribute.Name.ToLower();
            commandService.AddAsyncCommandHandler(commandName, async (entity, args) =>
            {
                var inGameCommand = serviceProvider.GetRequiredService(type) as IInGameCommand;
                if (inGameCommand == null)
                    throw new InvalidOperationException();

                await inGameCommand.Handle(entity, args);
            });
        }
    }
}
