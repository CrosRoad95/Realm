using System.Reflection;

namespace Realm.Server.Logic.Defaults;

public class DefaultCommandsLogic
{
    public DefaultCommandsLogic(RPGCommandService commandService, IServiceProvider serviceProvider, IEnumerable<IIngameCommand> ingameCommands)
    {
        foreach (var ingameCommand in ingameCommands)
        {
            var type = ingameCommand.GetType();
            var commandName = type.GetCustomAttribute<CommandNameAttribute>().Name.ToLower();
            commandService.AddCommandHandler(commandName, async (traceId, entity, args) =>
            {
                await (serviceProvider.GetRequiredService(type) as IIngameCommand).Handle(traceId, entity, args);
            });
        }
    }
}
